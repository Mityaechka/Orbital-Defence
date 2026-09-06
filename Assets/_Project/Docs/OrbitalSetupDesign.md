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

