---
name: orbital-defense-unity
description: Work on the local Orbital Defense Unity project: a 2D mobile-first space tower defense prototype with Unity Pipeline scene generation, MVP rules, file map, and workflow constraints.
---

# Orbital Defense Unity

Use this skill when the user asks to continue, inspect, modify, build, test, document, or reason about the local **Orbital Defense** Unity project.

## Project Identity

The project lives at `/Users/kaspi/Orbital Defense`. It is a single-player 2D mobile-first tower defense game in space:

- Player protects a mining planet from asteroid and alien waves.
- Buildings are placed on moons orbiting the central planet.
- Rotation/orbits should matter for placement decisions.
- Web build/playtest support is a target so the user can share builds with friends.

## Fixed Decisions

- Unity `6000.6.0f1`, 2D URP, C#, MonoBehaviour.
- No DOTS package for MVP; use ECS-like composition with small components.
- ScriptableObject data for buildings, enemies, waves, upgrades, level/time config.
- ScriptableObject data for world/moon layout.
- ScriptableObject data for localization text.
- UI Toolkit with UXML/USS for runtime UI.
- PNG sprites, vector-like 2D mobile style.
- Single-player only.
- MVP victory is clearing 10 waves.

## MVP Rules

Preserve these unless the user explicitly changes them:

- One planet and two moons.
- Each moon has its own visible orbit.
- Moon layout is configured through `Assets/_Project/ScriptableObjects/World/World_Default.asset`.
- Planet is the only damage target.
- Moons are only build platforms.
- Enemies fly to the planet center.
- Enemy/asteroid rewards are collected automatically.
- Building and upgrading are allowed only between waves.
- World time is paused between waves for comfortable building.
- Waves start by button.
- x1/x2 speed is supported during active wave play.

## Workflow

- When understanding or locating project code, use CodeGraph before `rg`, `grep`, `find`, or direct file reading. Start with `codegraph status "/Users/kaspi/Orbital Defense"`, then use `codegraph explore "<symbol or question>"` for symbols, call paths, and current line-numbered source. Exclude `Library/PackageCache` when searching for gameplay code and prefer names under `Assets/_Project`.
- If `.codegraph/` is missing or the index is stale, do not recreate or reindex it automatically unless the user explicitly asks; report the limitation and use normal project search as a fallback.
- Before editing scenes, prefabs, or ScriptableObject assets, use the Unity CLI skill and run `unity status --format json` from `/Users/kaspi/Orbital Defense`.
- If the Unity Editor is reachable, use Pipeline/editor commands for scene, prefab, and asset changes. Do not hand-edit `.unity`, `.prefab`, or `.asset` YAML while the live Editor is connected.
- C# source, markdown docs, `.gitignore`, and local notes can be edited normally.
- The generated prototype scene is rebuilt through the CLI command `orbital_defense_build_prototype`.
- Prefer changing `Assets/_Project/Scripts/Editor/PrototypeSceneBuilder.cs` when scene/prefab setup should be reproducible.
- Keep runtime gameplay systems independent of UI presenters.

## Important References

Read [references/project-map.md](references/project-map.md) when you need file locations, current systems, useful commands, asset notes, or MVP status.
