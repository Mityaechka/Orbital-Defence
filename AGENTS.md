# Orbital Defense Agent Notes

## Project

Orbital Defense is a single-player 2D mobile-first tower defense game made in Unity.

Core idea: the player protects a mining planet from waves of asteroids and aliens. Buildings can be placed on moons orbiting the planet. Rotation and orbital movement should matter for placement decisions.

## Fixed Stack

- Unity 6000.6.0f1 for the current local project.
- 2D URP.
- C# with MonoBehaviour.
- No DOTS/ECS package for MVP.
- ECS-like composition using small components.
- ScriptableObject data for levels, buildings, enemies, waves, upgrades, and time scale.
- ScriptableObject data for world/moon layout.
- UI Toolkit with UXML/USS for runtime UI.
- PNG sprites with a vector-like 2D mobile visual style.
- Single-player only.
- WebGL/Web build support is a playtest target, not the primary shipping target.

## Workflow

- Before editing scenes, prefabs, or ScriptableObject assets, run `unity status --project-path "/Users/kaspi/Orbital Defense" --format json`.
- If a live Unity Editor is reachable, use Unity Pipeline commands for scene/object/asset changes.
- Do not hand-edit `.unity`, `.prefab`, or `.asset` YAML while a live Editor is reachable.
- C# source, markdown docs, `.gitignore`, and project notes can be edited as normal files.
- Keep code scoped and data-driven. Prefer small reusable components over manager scripts that know everything.

## Project Skills

- Project-local skill copies live in `Assets/_Project/AgentSkills`.
- Current skill: `Assets/_Project/AgentSkills/orbital-defense-unity`.
- Installed Codex copy, when needed for automatic discovery: `/Users/kaspi/.codex/skills/orbital-defense-unity`.
- Keep the project copy updated when project structure, MVP rules, commands, or asset decisions change.

## MVP Rules

- One central planet.
- Two moons orbiting the planet.
- Each moon has its own visible orbit.
- Moon layout is configured through `Assets/_Project/ScriptableObjects/World/World_Default.asset`.
- Enemies fly toward the planet center in MVP.
- Moons are build platforms, not damage targets.
- Build and upgrade only between waves.
- Waves start by button.
- Resources from enemies/asteroids are collected automatically.
- Victory is clearing 10 waves.
- Time speed x1/x2 is part of the architecture.

## Folder Conventions

Project-owned content lives under `Assets/_Project`.

- Runtime scripts: `Assets/_Project/Scripts`
- Scenes: `Assets/_Project/Scenes`
- Prefabs: `Assets/_Project/Prefabs`
- ScriptableObjects: `Assets/_Project/ScriptableObjects`
- Art: `Assets/_Project/Art`
- Docs: `Assets/_Project/Docs`

## Code Style

- Use namespace `OrbitalDefense`.
- Use `[SerializeField]` private fields for inspector configuration.
- Expose read-only public properties for runtime state.
- UI calls controllers/systems, not world objects directly.
- Gameplay systems should not depend on UI classes.
- Avoid static singletons unless they make the prototype materially simpler.
- Keep comments short and only where they clarify non-obvious behavior.
