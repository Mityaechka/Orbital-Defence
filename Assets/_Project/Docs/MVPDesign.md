# Orbital Defense MVP Design

## Summary

Single-player 2D mobile tower defense in space. The player protects a mining planet from waves of asteroids and aliens by placing mines and cannons on rotating celestial bodies.

## Fixed Decisions

- Unity 6000.6.0f1, 2D URP, C#, MonoBehaviour.
- Single-player only.
- Vector-like 2D style exported as PNG sprites.
- One planet and two moons for MVP.
- Each moon has its own visible orbit and build slots.
- Moon layout is configured through a `WorldConfig` ScriptableObject.
- UI text is stored in a `LocalizationTable` ScriptableObject and shown in Russian by default.
- The Command Core in the planet center is the only damage target.
- Moons are only build platforms.
- Orbital stations are not damage targets.
- Enemies fly to the Command Core in the planet center.
- Resources are collected automatically.
- Building is allowed only between waves.
- The world is paused between waves to make building comfortable.
- Waves start by button.
- Time speed x1/x2 is supported.
- Victory after 10 waves.

## MVP Content

- Resource: Minerals.
- Base health: Command Core Integrity.
- Buildings: Mine, Cannon Turret.
- Enemies: Small Asteroid, Alien Scout.
- 10 configured waves.
- Global upgrade choice after waves, with rarer Command Core branch upgrades.

## Main Loop

1. Build phase.
2. Player places or sells buildings.
3. Player starts wave.
4. Enemies fly toward the Command Core.
5. Turrets shoot automatically.
6. Destroyed enemies grant Minerals automatically.
7. Wave ends when all enemies are gone.
8. Player chooses one of three offered upgrades.
9. Return to build phase.

## Web Playtest Target

The MVP should be exportable as a WebGL/Web build for easy sharing with friends. Desktop browser support is the primary web target; mobile browser support is a convenience target and must be tested separately because Unity Web builds can behave differently across mobile browsers and memory limits.
