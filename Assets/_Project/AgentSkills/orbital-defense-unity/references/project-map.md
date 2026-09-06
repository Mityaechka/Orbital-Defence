# Orbital Defense Project Map

## Root

Local project path: `/Users/kaspi/Orbital Defense`

Project content should live under `Assets/_Project` unless Unity/package files require another location.

Project-local skill copy: `Assets/_Project/AgentSkills/orbital-defense-unity`

Installed Codex skill copy for automatic discovery: `/Users/kaspi/.codex/skills/orbital-defense-unity`

## Core Documents

- `Assets/_Project/Docs/MVPDesign.md`: fixed game/design decisions.
- `Assets/_Project/Docs/ImplementationPlan.md`: milestone plan.
- `Assets/_Project/Docs/MVPStatus.md`: done/remaining checklist.
- `Assets/_Project/Docs/WebPlaytestPlan.md`: Web build/playtest plan.
- `AGENTS.md`: project-local agent notes.

## Main Scene And Generation

- Scene: `Assets/_Project/Scenes/MainGameplay.unity`
- Rebuild command: `unity command orbital_defense_build_prototype --format json`
- Builder: `Assets/_Project/Scripts/Editor/PrototypeSceneBuilder.cs`
- Web build helper: `Assets/_Project/Scripts/Editor/WebPlaytestBuilder.cs`

The builder creates/updates:

- Generated placeholder sprites in `Assets/_Project/Art/Sprites/Generated`
- Materials in `Assets/_Project/Art/Materials`
- Prefabs in `Assets/_Project/Prefabs`
- ScriptableObjects in `Assets/_Project/ScriptableObjects`
- `MainGameplay.unity`
- Editor build settings scene list

When adding scene objects, default to making the builder reproduce them.
Moon layout is data-driven by `Assets/_Project/ScriptableObjects/World/World_Default.asset`.

## Runtime System Map

- Bootstrap: `Scripts/Bootstrap/GameBootstrap.cs`
- Core state/time: `Scripts/Core/GameStateController.cs`, `GamePhase.cs`, `TimeScaleController.cs`, `TimeScaleConfig.cs`
- World config: `Scripts/World/WorldConfig.cs`
- Localization: `Scripts/Localization/GameLanguage.cs`, `LocalizationTable.cs`, `LocalizationService.cs`
- Economy: `Scripts/Economy/ResourceWallet.cs`
- Buildings: `Scripts/Buildings/BuildSystem.cs`, `Building.cs`, `BuildingConfig.cs`, `BuildCatalog.cs`, `MineProducer.cs`, `TurretWeapon.cs`
- Slots: `Scripts/Slots/BuildSlot.cs`, `BuildSlotSelector.cs`, `BuildSlotSelectionFeedback.cs`, `BuildSlotType.cs`
- Enemies: `Scripts/Enemies/EnemyConfig.cs`, `EnemyMover.cs`, `CoreDamageOnReach.cs`, `EnemyReward.cs`
- Waves: `Scripts/Waves/WaveSystem.cs`, `WaveConfig.cs`
- Combat: `Scripts/Combat/Health.cs`, `Projectile.cs`
- Upgrades: `Scripts/Upgrades/UpgradeSystem.cs`, `UpgradeConfig.cs`, `RunModifiers.cs`
- UI: `Scripts/UI/GameplayHudPresenter.cs`, `BuildPanelPresenter.cs`, `UpgradePanelPresenter.cs`, `CommandCorePanelPresenter.cs`, `GameOverPanelPresenter.cs`
- Visual feedback: `Scripts/Visuals/DamageFlash.cs`, `SpriteBurstEffect.cs`, `VisualEffectSpawner.cs`, `RangePreview.cs`

## Current MVP State

As of 2026-09-06:

- Playable prototype skeleton exists.
- One planet with a visible Command Core target, two moons, two visible moon orbits, 5 build slots total.
- Planet surface build slots are removed; moon surfaces are the build platforms.
- Build phase and upgrade choice pause world time.
- UI text is read from `Assets/_Project/ScriptableObjects/Localization/Localization_RU.asset`.
- Build/upgrade/sell work between waves.
- Waves, enemies, cannon combat, projectiles, rewards, core damage, victory, and defeat are implemented.
- Upgrade choice uses a weighted pool with normal upgrades and rarer Command Core branch upgrades.
- One upgrade offer can include at most one Command Core branch option.
- Command Core can be selected to inspect branch levels and current shield blocks.
- Cannon range preview is shown from the selected slot or built Cannon.
- Orbital Booster is removed from the current playable MVP.
- UI exists but still needs polish for mobile.
- Kenney Space Shooter Redux assets are imported under `Assets/_Project/ThirdParty/Kenney/SpaceShooterRedux` and integrated for enemies, projectile, building icons, and starfield background.
- First Web build has been produced at `Builds/WebPlaytest`.
- Web build HTTP smoke-test passed locally.
- The project is not currently a git repository.

## Third-Party Assets

Kenney Space Shooter Redux:

- Folder: `Assets/_Project/ThirdParty/Kenney/SpaceShooterRedux`
- Local note: `Assets/_Project/ThirdParty/Kenney/README.md`
- License file: `Assets/_Project/ThirdParty/Kenney/SpaceShooterRedux/license.txt`
- License: CC0 / Public Domain.

Current integrated sprite choices in the prototype builder:

- Small Asteroid: `PNG/Meteors/meteorBrown_big3.png`
- Alien Scout: `PNG/Enemies/enemyBlue4.png`
- Projectile: `PNG/Lasers/laserBlue08.png`
- Mine icon: `PNG/Power-ups/powerupYellow_bolt.png`
- Cannon icon: `PNG/Parts/gun09.png`
- Background: `Backgrounds/black.png`

## Useful Commands

Run from `/Users/kaspi/Orbital Defense`.

```bash
unity status --format json
unity command orbital_defense_build_prototype --format json
unity command orbital_defense_build_web_playtest --project-path "/Users/kaspi/Orbital Defense" --timeout 1200 --format json
unity build "/Users/kaspi/Orbital Defense" --target WebGL --execute-method OrbitalDefense.EditorTools.WebPlaytestBuilder.Build --output-path "Builds/WebPlaytest" --allow-dirty-build --timeout 1200 --format json
unity command eval '<C# code>' --format json
```

Check console status through Unity Pipeline when needed. If the Editor is unavailable or in Safe Mode, fix C# compile errors first.

## Near-Term Work Queue

- Polish phase/button/upgrade text.
- Improve build panel ergonomics for phone screens.
- Add basic audio feedback.
- Play through all 10 waves and tune balance.
- Open the Web build in a desktop browser and verify mouse playability.
- Check touch input in a mobile browser.
