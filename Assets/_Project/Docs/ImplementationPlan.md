# Orbital Defense Implementation Plan

## Milestone 1: Project Foundation

- Add project rules in `AGENTS.md`.
- Create `Assets/_Project` folder structure.
- Add runtime script skeletons.
- Confirm Unity Pipeline connection.
- Keep generated tutorial/readme assets removed.

## Milestone 2: Scene Skeleton

- Create `MainGameplay.unity`.
- Add root objects: `Bootstrap`, `GameWorld`, `PlanetRoot`, `MoonOrbitRoot`, `PlanetOrbitRoot`, `EnemySpawnRoot`, `ProjectileRoot`, `VfxRoot`, `UIRoot`.
- Add placeholder visuals for planet, moon, slots, orbit lines.
- Add `SelfRotator`, `OrbitMover`, and build slots.

## Milestone 3: Build And Economy

- Add Minerals wallet.
- Add Mine building and production.
- Add build slot selection and simple placement.
- Restrict building to build phase.

## Milestone 4: Waves And Core Damage

- Add enemy configs.
- Add wave configs.
- Spawn enemies outside orbit.
- Move enemies toward `PlanetCenter`.
- Damage Core Integrity on reach.

## Milestone 5: Combat

- Add Cannon targeting.
- Add projectiles.
- Add health/death/reward flow.
- Add simple hit and death feedback.

## Milestone 6: UI

- Add gameplay HUD.
- Add build panel.
- Add building panel.
- Add upgrade choice panel.
- Add win/lose panel.
- Add x1/x2 speed button.

## Milestone 7: Web Playtest

- Add WebGL/Web as a supported test target.
- Add build helper for web playtest.
- Produce a local web build.
- Test desktop browser first.
- Test mobile browser separately and note limitations.

