# Web Playtest Plan

## Goal

Make the MVP easy to share as a browser build for friends to test on PC and, where possible, mobile browsers.

## Target Priority

1. Desktop browser playtest.
2. Mobile browser smoke test.
3. Native Android build for serious mobile testing.

## Unity Setup

- Install Web Build Support/WebGL module for the active Unity Editor.
- Keep Android as the main mobile target.
- Treat Web as an extra playtest target.
- Use disabled WebGL compression for early local playtests, so the build works with a simple static HTTP server without Brotli/Gzip response headers.

## Build Output

Use a dedicated build folder outside `Assets`:

```text
Builds/WebPlaytest/
```

Build command:

```bash
unity command orbital_defense_build_web_playtest --project-path "/Users/kaspi/Orbital Defense" --format json
```

For the first WebGL build, use a longer Unity Pipeline timeout because shader/code compilation can take several minutes:

```bash
unity command orbital_defense_build_web_playtest --project-path "/Users/kaspi/Orbital Defense" --timeout 1200 --format json
```

Batch-mode fallback:

```bash
unity build "/Users/kaspi/Orbital Defense" --target WebGL --execute-method OrbitalDefense.EditorTools.WebPlaytestBuilder.Build --output-path "Builds/WebPlaytest" --allow-dirty-build --timeout 1200 --format json
```

## Sharing Options

- Itch.io private/unlisted page.
- GitHub Pages for static hosting.
- Netlify/Vercel static deploy.
- Local network testing with a simple static server.

## MVP Web Constraints

- Keep initial download small.
- Avoid large textures/audio.
- Test memory use on mobile browsers.
- Use simple touch input that also works with mouse.
- Do not depend on native mobile features for MVP.

## Done Criteria

- Web build is created at `Builds/WebPlaytest`.
- Web build responds through a local HTTP server.
- Web build starts in desktop Chrome/Safari.
- Game is playable with mouse.
- Game is playable with touch on at least one mobile browser.
- Build link can be shared privately.
