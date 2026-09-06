# Orbital Setup Design

## Purpose

Orbital Setup is the preparation mechanic that lets the player reposition the two moons between waves. It turns moon placement from a visual detail into a readable defensive decision while preserving fair outcomes.

The player should be able to answer three questions before starting a wave:

- Where will the next enemies come from?
- Which moon should cover that threat?
- What part of the defense becomes weaker after repositioning?

## Core Rule

Moon position may be changed only between waves. When a wave starts, moon positions are locked until the wave ends.

Buildings, build slots, and their orbital position move together with the moon. During a wave, enemies follow the previewed trajectories and cannot create an unexpected coverage gap by causing the moon to rotate away.

Moon rotation may continue as a cosmetic animation, but gameplay calculations use the locked defensive position for the entire wave.

## Updated Game Loop

```text
Wave complete
    -> Upgrade choice
    -> Build phase
    -> Inspect next wave forecast
    -> Reposition moons
    -> Rebuild or upgrade buildings
    -> Review coverage
    -> Lock orbits
    -> Start wave
    -> Wave combat
```

`UpgradeChoice` still returns to `BuildPhase`. `BuildPhase` is extended with an Orbital Setup step; it does not become a separate gameplay phase for the first implementation.

## Player Actions

During BuildPhase the player can:

- drag a moon around its configured orbit;
- use left/right rotate controls for precise positioning;
- select a moon and inspect its slots and buildings;
- build, sell, and upgrade buildings as before;
- compare current coverage with the forecasted enemy trajectories;
- restore the previous moon position before locking;
- lock both moon positions and start the wave.

The first version should not charge minerals for repositioning. The meaningful cost is coverage tradeoff: moving one moon toward a threat leaves another sector less protected.

## Wave Forecast

Before the wave starts, the HUD should show:

- wave number;
- enemy groups and counts;
- attack direction or directions;
- approximate trajectory sectors;
- special wave rule, if any;
- reward and expected risk where available.

The world view should show:

- enemy trajectory lines or sector cones;
- a ghost preview of the selected moon position;
- cannon coverage arcs or circles;
- covered trajectory sections in green;
- weak or uncovered sections in amber/red;
- a clear warning when no cannon covers a high-risk section.

The forecast must be deterministic after the player locks the orbits. Visual uncertainty is acceptable for the exact spawn point, but not for the attack sector or the relevant coverage decision.

## Moon Positioning

For the first implementation, moons move only along their existing configured orbit. Do not add radius changes, orbit editing, or independent slot rotation yet.

Each moon keeps:

- its configured orbit radius;
- its current angular position;
- its existing slot layout;
- its buildings as children of the moon root.

The player can rotate a moon freely during BuildPhase, subject to a small input step or drag snap if precision becomes difficult on mobile.

Recommended initial controls:

- drag directly on the moon to rotate it;
- optional `-15 deg` and `+15 deg` buttons for touch precision;
- `Reset Position` to undo changes made during the current setup;
- `Lock Orbits` to commit the configuration.

## Locking Behavior

When the player presses `Lock Orbits`:

1. Store the current angular position of both moons.
2. Store the wave forecast used for the preview.
3. Disable moon reposition input.
4. Disable build, sell, and building upgrades.
5. Keep cannon targeting and enemy movement active.
6. Start the wave when the player presses `Start Wave`, or combine both buttons if the UX is clearer.

When the wave ends:

1. Stop the current wave.
2. Unlock moon repositioning.
3. Return to the regular upgrade/build flow.
4. Preserve the last moon positions as the starting layout for the next setup.

For the first version, `Lock Orbits` and `Start Wave` may be a single `Confirm Setup` action. A separate lock state is preferable if the preview needs more testing or if future setup actions should remain available after locking.

## Fairness Requirements

The mechanic must satisfy these rules:

- no moon movement may unexpectedly remove a planned firing opportunity during a wave;
- the player must see the relevant attack sector before committing the layout;
- the player must see the effective cannon coverage after moving a moon;
- an uncovered sector must be a visible consequence of the player's choice;
- enemy lanes must not change after the wave starts unless a special event clearly announces it;
- a failed defense should be explainable from the final locked layout;
- the player must be able to undo repositioning before the wave starts.

The game should never communicate that a moon position is safe and then invalidate that promise through passive rotation.

## Tactical Meaning

Moon position should create tradeoffs rather than a single correct alignment.

Examples:

- the large moon has stronger cannons but takes longer to reposition;
- the small moon is easier to align with fast scouts;
- placing mines on the side facing the forecasted attack gives income but reduces immediate firepower;
- covering the outer trajectory gives more time to shoot, while covering the inner trajectory protects against late leaks;
- aligning both moons to one sector creates a strong defense there but leaves the opposite sector exposed.

These differences should be introduced through data and balance after the basic mechanic works. The first implementation only needs position, preview, and lock behavior.

## Roguelite Integration

Later upgrade protocols can modify Orbital Setup rules:

- `Synchronized Orbits`: bonus when both moons cover the same sector.
- `Distributed Defense`: bonus when the moons cover different sectors.
- `Mobile Station`: one moon can be repositioned once during a wave.
- `Orbital Stabilizer`: the first coverage gap each wave is ignored.
- `Deep Orbit`: increased range, but the moon has fewer available build slots.
- `Rapid Alignment`: one moon receives a free additional rotation after a crisis wave.

These are post-slice extensions. They must not be implemented before the base lock-and-preview loop is reliable.

## Runtime Responsibilities

The implementation should remain split between gameplay systems and presentation:

- `GameStateController`: keeps build/wave permissions and phase transitions;
- moon orbit component: stores and applies the current angular position;
- orbital setup controller: accepts player reposition input and handles reset/lock;
- wave system: exposes the current forecast and owns the locked wave configuration;
- coverage preview presenter: draws trajectories, ranges, warnings, and ghost positions;
- UI presenter: exposes forecast text, reset, lock, and start controls;
- building systems: continue to treat buildings as children of their build slots.

Gameplay systems should not depend on UI classes. The wave system should receive a locked setup state or read the locked moon positions through a small data model rather than querying UI objects.

## Data Requirements

The first version can use the existing `WorldConfig` and `WaveConfig` data with minimal additions.

Potential data fields:

- `initialAngleDegrees` for each moon, if not already available;
- `allowPlayerReposition` on the world or moon config;
- `repositionSnapDegrees`, defaulting to 15 or 30;
- wave attack sectors or group angles, reusing existing spawn group angles where possible;
- a forecast display flag for special wave rules;
- optional moon role or positioning modifiers for later balancing.

Do not duplicate wave direction data in the UI. The preview should read the same spawn group information used by the wave spawner.

## Implementation Order

### Slice 1: Safe repositioning

- expose moon angular position;
- allow rotation only during BuildPhase;
- keep buildings and slots attached to the moon;
- disable input during Wave and UpgradeChoice;
- confirm positions survive the transition into the wave.

### Slice 2: Forecast and coverage

- derive attack sectors from existing wave spawn groups;
- draw trajectory indicators;
- show cannon ranges after moon movement;
- add weak-coverage feedback;
- add reset and confirm controls.

### Slice 3: Wave lock

- capture both moon positions at confirmation;
- prevent gameplay-relevant moon movement during the wave;
- verify that the wave uses the locked setup;
- unlock after completion, defeat, or restart.

### Slice 4: Playtest balance

- create waves where repositioning matters;
- ensure at least two reasonable defensive layouts exist;
- test mouse and touch controls;
- test that a player can understand why a leak occurred;
- tune orbit speed, snap angle, preview clarity, and setup duration.

### Slice 5: Roguelite protocols

- add one protocol that rewards aligned moons;
- add one protocol that rewards separated coverage;
- add one protocol that gives limited mid-wave control;
- verify that protocols change decisions instead of only increasing stats.

## Acceptance Criteria

The feature is ready for the first playtest when:

- the player can move either moon between waves;
- all buildings and slots move with their moon;
- moon movement is impossible during an active wave;
- the next wave's attack direction is visible before setup confirmation;
- cannon coverage updates immediately while a moon moves;
- the player can undo a positioning mistake before starting;
- the same locked setup produces the same enemy directions in repeated tests;
- no enemy can bypass a moon because of an unannounced passive rotation;
- the UI makes clear whether the player is configuring, locked, or fighting;
- a short test wave demonstrates a meaningful difference between at least two moon positions.

## Open Design Decisions

These decisions should be made after the base slice is playable:

- whether `Lock Orbits` and `Start Wave` are separate buttons;
- whether moon rotation snaps to fixed angles or remains continuous;
- whether the large and small moons have different rotation limits;
- whether a moon can be repositioned once during a wave through a rare protocol;
- whether coverage depends only on cannon range or also on moon-specific firing arcs;
- whether special waves are allowed to change direction after a warning.

## Systems And Code Changes

### `OrbitMover`

File: `Assets/_Project/Scripts/Orbits/OrbitMover.cs`

The current component moves a moon automatically every frame. This must change: gameplay-relevant moon movement is player-controlled during `BuildPhase` and locked during `Wave`.

Add:

- `SetAngle(float angleDegrees)`;
- `RotateBy(float deltaDegrees)`;
- `ResetAngle()`;
- `LockPosition()` and `UnlockPosition()`;
- `IsLocked` state;
- `AngleChanged` event;
- angle normalization to `0..360` degrees.

The existing automatic `Update()` movement should no longer change the gameplay position. Cosmetic moon rotation may remain separate from orbital position.

Suggested API:

```csharp
public float CurrentAngleDegrees { get; private set; }
public bool IsLocked { get; private set; }

public void SetAngle(float angleDegrees);
public void RotateBy(float deltaDegrees);
public void ResetAngle();
public void LockPosition();
public void UnlockPosition();
```

### New `OrbitalSetupController`

File: `Assets/_Project/Scripts/Orbits/OrbitalSetupController.cs`

This controller coordinates all moons and owns the setup lifecycle. It should not know about UI visuals.

Responsibilities:

- register all spawned moons;
- keep the current and previous angles;
- allow repositioning only in `BuildPhase`;
- reset changes made during the current setup;
- lock both moons for the active wave;
- unlock them after the wave ends;
- expose setup readiness and change events.

Suggested API:

```csharp
public bool CanReposition { get; }
public bool IsReadyForWave { get; }

public void RegisterMoon(OrbitMover mover, int index, string displayName);
public void BeginSetup();
public void ResetPositions();
public void LockForWave();
public void UnlockAfterWave();
public bool TrySetMoonAngle(int moonIndex, float angleDegrees);
public bool TryRotateMoon(int moonIndex, float deltaDegrees);
```

### `GameStateController`

File: `Assets/_Project/Scripts/Core/GameStateController.cs`

No new game phase is required for the first implementation. `BuildPhase` includes both construction and orbital setup.

Optionally add:

```csharp
public bool CanConfigureOrbit => CurrentPhase == GamePhase.BuildPhase;
```

Expected permissions:

| Game phase | Build | Move moons | Lock setup |
| --- | --- | --- | --- |
| `BuildPhase` | yes | yes | yes |
| `Wave` | no | no | no |
| `UpgradeChoice` | no | no | no |
| `Victory` / `Defeat` | no | no | no |

### `WaveSystem`

File: `Assets/_Project/Scripts/Waves/WaveSystem.cs`

`StartNextWave()` must verify and lock the orbital setup before entering `Wave`.

Required flow:

1. check `OrbitalSetupController.IsReadyForWave`;
2. capture the two current moon angles;
3. call `LockForWave()`;
4. enter `GamePhase.Wave`;
5. spawn the wave using the locked setup;
6. unlock the moons after wave completion.

The current `WaveConfig` direction data should remain the source for the forecast. Do not duplicate wave angles in UI objects.

Add read-only forecast accessors such as:

```csharp
public WaveConfig GetNextWave();
public IReadOnlyList<WaveStageForecast> GetNextWaveForecast();
```

### `MoonSpawner`

File: `Assets/_Project/Scripts/World/MoonSpawner.cs`

When a moon is created, keep the `OrbitMover` reference and register it with `OrbitalSetupController`.

The existing hierarchy already solves building movement:

```text
Moon
└── SurfaceSlots
    └── BuildSlot
        └── Building
```

Moving the moon therefore moves its slots and buildings without changes to building placement.

### Input

New file: `Assets/_Project/Scripts/Input/OrbitalSetupInput.cs`

This component translates mouse or touch input into controller calls.

Required input:

- select a moon;
- drag the moon around the planet;
- rotate by fixed steps, initially `15` or `30` degrees;
- reset the current setup;
- prevent input outside `BuildPhase`.

Input must call `OrbitalSetupController`, not modify `Transform` or `CurrentAngleDegrees` directly.

### Coverage Calculation

New file: `Assets/_Project/Scripts/Orbits/OrbitalCoverageCalculator.cs`

This system evaluates how well the current moon and cannon layout covers the next wave.

Inputs:

- moon positions;
- build slots and cannon positions;
- cannon range;
- next wave spawn groups;
- group angle and angle spread.

Outputs:

- covered or uncovered sectors;
- coverage percentage;
- responsible cannon for a sector;
- high-risk gaps.

The first implementation may use a simplified test: a cannon covers a sector when the sector trajectory falls within its range. More accurate interception timing can be added after the mechanic is playable.

### `WaveDirectionWarningPresenter`

File: `Assets/_Project/Scripts/UI/WaveDirectionWarningPresenter.cs`

The current warning shows a single `!` and direction. Extend it to display the next-wave forecast:

- attack directions;
- enemy group counts;
- trajectory sectors;
- special wave rule;
- coverage warnings.

The existing active-wave warning can remain; the new forecast is shown during `BuildPhase`.

### New `OrbitalSetupPresenter`

File: `Assets/_Project/Scripts/UI/OrbitalSetupPresenter.cs`

Responsibilities:

- show the selected moon;
- display its angle and name;
- expose rotate controls;
- provide `Reset`;
- provide `Lock Orbits` and/or `Start Wave`;
- show `Configuring`, `Ready`, and `Locked` states;
- display coverage feedback from `OrbitalCoverageCalculator`.

The first UI can combine lock and start into one action, but the code should keep separate methods:

```csharp
public void OnLockOrbitsClicked();
public void OnStartWaveClicked();
```

### `GameplayHudPresenter`

File: `Assets/_Project/Scripts/UI/GameplayHudPresenter.cs`

The existing start button currently calls `WaveSystem.StartNextWave()` directly. It must first commit the orbital setup or delegate that action to `OrbitalSetupPresenter`.

Preferred code separation:

```text
Lock Orbits -> OrbitalSetupController.LockForWave()
Start Wave   -> WaveSystem.StartNextWave()
```

For the first slice these can share one visible button while remaining separate methods internally.

### `BuildSystem`, `BuildSlot`, and `Building`

Files:

- `Assets/_Project/Scripts/Buildings/BuildSystem.cs`;
- `Assets/_Project/Scripts/Slots/BuildSlot.cs`;
- `Assets/_Project/Scripts/Buildings/Building.cs`.

No major changes are needed. The existing parent hierarchy makes slots and buildings follow the moon automatically. Existing `CanBuild` checks already prevent building, selling, and upgrading during the wave.

Only add a setup-lock check if `Lock Orbits` is implemented as a separate state inside `BuildPhase`.

### `GameBootstrap`

File: `Assets/_Project/Scripts/Bootstrap/GameBootstrap.cs`

Changes should be minimal:

- find or receive `OrbitalSetupController`;
- enable repositioning in `BuildPhase`;
- disable repositioning in `Wave`, `UpgradeChoice`, `Victory`, and `Defeat`;
- keep current pause behavior between waves.

Manual drag should not depend on `Time.timeScale`; use unscaled input timing where smooth movement is needed.

### `PrototypeSceneBuilder`

File: `Assets/_Project/Scripts/Editor/PrototypeSceneBuilder.cs`

Because the gameplay scene is generated, the builder must create and wire:

- `OrbitalSetupController`;
- orbital input component;
- coverage calculator;
- setup presenter;
- reset, lock, and start controls;
- forecast UI;
- localization keys.

Otherwise a regenerated prototype scene will lose the new references.

### Systems Not Directly Changed

The first implementation should not modify:

- `EnemyMover`;
- `EnemyReward`;
- `CoreIntegrity`;
- `TurretWeapon`;
- `Projectile`;
- `MineProducer`;
- `UpgradeSystem`;
- `UpgradePanelPresenter`;
- `CommandCoreUpgrade`.

Roguelite upgrades that modify orbital rules can be added later through `UpgradeSystem` and `RunModifiers`.

## Implementation Sequence

### Step 1: Controlled moons

- change `OrbitMover` to support manual angle changes;
- add `OrbitalSetupController`;
- register spawned moons from `MoonSpawner`;
- verify that slots and buildings follow moons.

### Step 2: Player input

- add `OrbitalSetupInput`;
- implement moon selection and drag;
- add fixed-angle controls;
- add reset behavior.

### Step 3: Wave locking

- update `WaveSystem` to lock before spawning;
- prevent orbital movement during `Wave`;
- unlock after completion, defeat, or restart;
- add tests for phase permissions.

### Step 4: Forecast and coverage

- add `OrbitalCoverageCalculator`;
- extend the wave forecast presenter;
- add visual sectors, ranges, and uncovered warnings;
- expose lock/readiness state in the HUD.

### Step 5: Generated scene wiring

- update `PrototypeSceneBuilder`;
- add localization entries;
- regenerate `MainGameplay`;
- verify the generated scene from a clean rebuild.

### Step 6: Balance and playtest

- tune snap angle and drag sensitivity;
- create waves where repositioning matters;
- ensure at least two viable layouts exist;
- verify that every leak can be explained from the locked layout;
- test desktop mouse and mobile touch input.

## Recommended New Files

```text
Assets/_Project/Scripts/Orbits/OrbitalSetupController.cs
Assets/_Project/Scripts/Input/OrbitalSetupInput.cs
Assets/_Project/Scripts/Orbits/OrbitalCoverageCalculator.cs
Assets/_Project/Scripts/UI/OrbitalSetupPresenter.cs
```

## Architecture Boundary

Keep responsibilities separated:

```text
GameStateController
    -> OrbitalSetupController
        -> OrbitMover

WaveSystem
    -> wave forecast data

OrbitalCoverageCalculator
    -> coverage result

OrbitalSetupInput
    -> OrbitalSetupController

OrbitalSetupPresenter
    -> displays state and calls controller actions
```

The orbital gameplay system must not depend on UI classes, and the UI must not manipulate moon transforms directly.
