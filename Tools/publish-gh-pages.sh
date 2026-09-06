#!/usr/bin/env bash
set -euo pipefail

ROOT="$(git rev-parse --show-toplevel)"
UNITY_BIN="${UNITY_BIN:-unity}"
UNITY_TIMEOUT="${UNITY_TIMEOUT:-1200}"
BUILD_OUTPUT="${BUILD_OUTPUT:-$ROOT/Builds/WebPlaytest}"
BUILD_METHOD="${BUILD_METHOD:-OrbitalDefense.EditorTools.WebPlaytestBuilder.Build}"
REMOTE="${REMOTE:-origin}"
PAGES_BRANCH="${PAGES_BRANCH:-gh-pages}"
CNAME_SOURCE="${CNAME_SOURCE:-$ROOT/CNAME}"

if ! command -v "$UNITY_BIN" >/dev/null 2>&1; then
  echo "unity command not found: $UNITY_BIN" >&2
  exit 1
fi

if ! git config user.name >/dev/null || ! git config user.email >/dev/null; then
  echo "Configure git user.name and user.email before publishing." >&2
  exit 1
fi

if git ls-remote --exit-code --heads "$REMOTE" "$PAGES_BRANCH" >/dev/null 2>&1; then
  git fetch "$REMOTE" "+refs/heads/$PAGES_BRANCH:refs/heads/$PAGES_BRANCH" >/dev/null 2>&1 || true
fi

PUBLISH_WORKTREE="$(mktemp -d "${TMPDIR:-/tmp}/orbital-defense-gh-pages.XXXXXX")"
WORKTREE_READY=0

cleanup() {
  if [[ "$WORKTREE_READY" -eq 1 ]]; then
    git worktree remove --force "$PUBLISH_WORKTREE" >/dev/null 2>&1 || rm -rf "$PUBLISH_WORKTREE"
  else
    rm -rf "$PUBLISH_WORKTREE"
  fi
}

trap cleanup EXIT

echo "Building WebGL player into $BUILD_OUTPUT"
"$UNITY_BIN" build "$ROOT" \
  --target WebGL \
  --execute-method "$BUILD_METHOD" \
  --output-path "$BUILD_OUTPUT" \
  --allow-dirty-build \
  --timeout "$UNITY_TIMEOUT" \
  --format json

if git show-ref --verify --quiet "refs/heads/$PAGES_BRANCH"; then
  git worktree add --force "$PUBLISH_WORKTREE" "$PAGES_BRANCH" >/dev/null
else
  git worktree add --detach "$PUBLISH_WORKTREE" HEAD >/dev/null
  git -C "$PUBLISH_WORKTREE" checkout --orphan "$PAGES_BRANCH" >/dev/null
  git -C "$PUBLISH_WORKTREE" rm -rf . --ignore-unmatch >/dev/null 2>&1 || true
fi

WORKTREE_READY=1

# Replace the branch contents with the fresh build output.
find "$PUBLISH_WORKTREE" -mindepth 1 -maxdepth 1 ! -name .git -exec rm -rf {} +
rsync -a --delete "$BUILD_OUTPUT"/ "$PUBLISH_WORKTREE"/
touch "$PUBLISH_WORKTREE/.nojekyll"

if [[ -f "$CNAME_SOURCE" ]]; then
  cp "$CNAME_SOURCE" "$PUBLISH_WORKTREE/CNAME"
fi

git -C "$PUBLISH_WORKTREE" add -A

if git -C "$PUBLISH_WORKTREE" diff --cached --quiet; then
  echo "Nothing changed, gh-pages already matches the latest build."
  exit 0
fi

git -C "$PUBLISH_WORKTREE" commit -m "Publish WebGL build $(date -u +%Y-%m-%d)"
git -C "$PUBLISH_WORKTREE" push "$REMOTE" "$PAGES_BRANCH"

echo "Published $BUILD_OUTPUT to $REMOTE/$PAGES_BRANCH"
