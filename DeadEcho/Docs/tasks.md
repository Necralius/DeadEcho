# Dead Echo — Development Backlog and Task Breakdown

This document breaks down the initial Dead Echo development plan into practical milestones, tasks, descriptions, and acceptance criteria.

The goal is to prioritize a playable, testable vertical slice before expanding content or building complex systems.

---

## Development Principle

Every task should produce something playable, testable, reusable, or clearly validated.

The initial target is not to build the full game. The initial target is to build a short, scary, functional, and technically stable vertical slice.

---

## Architecture Assumptions

The task breakdown assumes the previously established architecture:

```text
Project.Core
Project.Gameplay
Project.AI
Project.Audio
Project.UI
Project.Narrative
Project.Infrastructure
Project.Editor
```

### Dependency Rules

```text
Core must not reference Gameplay, UI, AI, Audio, Narrative, or Infrastructure implementations.
Gameplay may depend on Core contracts and shared events.
UI communicates through services, contracts, or events.
AI should not depend directly on concrete Player implementations when avoidable.
Audio should be event-driven.
Narrative should trigger gameplay through events or high-level services.
Infrastructure implements technical services such as loading, save, configuration, and Addressables.
```

### Mandatory Technical Rules

```text
Use Assembly Definitions from the start.
Avoid FindObjectOfType as a default dependency strategy.
Use VContainer for composition, not as a replacement for clean design.
Avoid registering every MonoBehaviour in the container.
Keep gameplay logic out of UI.
Every major system must have a small validation scene, debug mode, or clear test flow.
The vertical slice must come before large content expansion.
```

---

# Milestone 0 — Project Foundation

## Goal

Create the technical foundation required to support a scalable but practical Unity project.

---

## Task 0.1 — Create Unity Project and Configure Base Settings

### Description

Create the Dead Echo Unity project using the selected Unity version and configure the base rendering, input, quality, and folder structure settings.

### Scope

- Create the Unity project.
- Configure URP.
- Configure project naming conventions.
- Create the initial folder structure under `Assets/Project`.
- Configure basic quality settings.
- Configure initial scenes.

### Acceptance Criteria

- The project opens without errors.
- URP is configured and active.
- The base folder structure exists.
- The initial boot scene is present.
- The project is ready for assembly definition setup.

---

## Task 0.2 — Create Assembly Definitions

### Description

Create the initial Assembly Definition files to enforce modular boundaries between Core, Gameplay, AI, Audio, UI, Narrative, Infrastructure, and Editor code.

### Scope

- Create `Project.Core.asmdef`.
- Create `Project.Gameplay.asmdef`.
- Create `Project.AI.asmdef`.
- Create `Project.Audio.asmdef`.
- Create `Project.UI.asmdef`.
- Create `Project.Narrative.asmdef`.
- Create `Project.Infrastructure.asmdef`.
- Create `Project.Editor.asmdef`.
- Configure allowed references.

### Acceptance Criteria

- The project compiles with all assemblies enabled.
- Core has no dependency on Gameplay, AI, UI, Audio, Narrative, or Infrastructure implementations.
- No circular assembly references exist.
- A simple class can compile inside each assembly.

---

## Task 0.3 — Install and Configure VContainer

### Description

Install VContainer and configure the first project lifetime scope for global services and scene-level composition.

### Scope

- Install VContainer.
- Create the root lifetime scope.
- Create conventions for scene lifetime scopes.
- Register the first core services.
- Document where VContainer should and should not be used.

### Acceptance Criteria

- VContainer is installed and compiling.
- The root lifetime scope is present in the boot scene.
- At least one service can be registered and resolved.
- There is no dependency resolution through scattered scene searches.

---

## Task 0.4 — Implement Initial App State Machine

### Description

Create the base application state machine responsible for controlling the main game flow.

### Scope

- Create `IAppState` or equivalent state contract.
- Create `AppStateMachine`.
- Create `BootState`.
- Create `MainMenuState`.
- Create `LoadingState`.
- Create `GameplayState`.
- Create `PauseState` placeholder.
- Create `DeathState` placeholder.

### Acceptance Criteria

- The game starts in `BootState`.
- The state machine can transition to `MainMenuState`.
- The state machine can transition to `LoadingState`.
- The state machine can enter `GameplayState` after scene loading.
- State transitions are logged in the console.

---

## Task 0.5 — Implement Scene Loading Service

### Description

Create a scene loading service used by the state machine to load gameplay and menu scenes in a controlled way.

### Scope

- Create `ISceneLoader` contract.
- Implement scene loading service.
- Support loading by scene name or scene reference.
- Add loading progress reporting.
- Integrate with `LoadingState`.

### Acceptance Criteria

- The state machine can load the gameplay test scene.
- Loading progress can be queried or reported.
- Scene loading is not called directly from random gameplay classes.
- Failed scene loading is handled with a clear error log.

---

## Task 0.6 — Implement Input Service

### Description

Create the initial input abstraction using Unity's New Input System.

### Scope

- Create input actions.
- Add actions for Move, Look, Run, Crouch, Interact, Use, and Pause.
- Create `IInputService` contract.
- Create implementation for reading player input.
- Ensure input can be enabled/disabled by game state.

### Acceptance Criteria

- Input actions are configured.
- Gameplay input can be enabled and disabled.
- UI or pause states can block gameplay input.
- Player systems do not directly depend on raw input actions everywhere.

---

## Task 0.7 — Create Technical Playground Scene

### Description

Create a simple playground scene used to validate movement, interaction, AI, objectives, UI, and audio systems before production-level map work begins.

### Scope

- Add floor and walls.
- Add simple lighting.
- Add player spawn point.
- Add one interactable object.
- Add one enemy placeholder.
- Add one objective trigger.

### Acceptance Criteria

- The scene can be loaded from the state machine.
- The player can spawn in the scene.
- The scene is clearly separated from final production maps.
- The scene can be reused for system testing.

---

# Milestone 1 — Player Controller and Base Game Feel

## Goal

Create a responsive first-person player controller suitable for an immersive horror/survival experience.

---

## Task 1.1 — Create Player Prefab

### Description

Create the base player prefab with required components for movement, camera, interaction, health, and future system integration.

### Scope

- Create player root prefab.
- Add character controller or movement body.
- Add camera pivot.
- Add FPS camera.
- Add interaction origin.
- Add placeholder player model or capsule.
- Add component references through serialized private fields.

### Acceptance Criteria

- Player prefab can be spawned in the playground scene.
- Camera follows player correctly.
- Required references are assigned through Inspector or composition.
- The prefab is reusable across scenes.

---

## Task 1.2 — Implement Basic Movement

### Description

Implement basic first-person movement for walking, acceleration, deceleration, gravity, and grounded detection.

### Scope

- Implement walking movement.
- Implement gravity.
- Implement grounded check.
- Add basic slope handling.
- Add movement configuration data.

### Acceptance Criteria

- Player can move around the playground scene.
- Movement feels responsive.
- Player does not float or fall through the floor.
- Movement parameters can be adjusted without changing code.

---

## Task 1.3 — Implement Camera Look

### Description

Implement first-person camera look with mouse input, vertical clamp, and sensitivity settings.

### Scope

- Implement horizontal look.
- Implement vertical look.
- Clamp vertical rotation.
- Add sensitivity configuration.
- Prepare for future settings menu integration.

### Acceptance Criteria

- Mouse look works correctly.
- Vertical rotation is clamped.
- Sensitivity can be adjusted.
- Camera does not roll or drift unexpectedly.

---

## Task 1.4 — Implement Running

### Description

Add running behavior to the player controller, including movement speed changes and optional FOV feedback.

### Scope

- Add run input.
- Add run speed.
- Add smooth transition between walk and run.
- Add optional camera FOV change.

### Acceptance Criteria

- Player can run while holding the configured input.
- Running speed is configurable.
- Movement returns to normal when run input is released.
- Running does not break camera or collision behavior.

---

## Task 1.5 — Implement Crouch

### Description

Add crouch behavior to support stealth, navigation, and tension-based gameplay.

### Scope

- Add crouch input.
- Change player height or camera height.
- Add crouch movement speed.
- Prevent standing up under blocked space if applicable.

### Acceptance Criteria

- Player can crouch and uncrouch.
- Crouch affects movement speed.
- Camera height changes smoothly.
- Player cannot exploit crouch to clip through objects.

---

## Task 1.6 — Implement Stamina System

### Description

Create a simple stamina system used by running and future physical actions.

### Scope

- Add max stamina.
- Consume stamina while running.
- Recover stamina when not running.
- Add exhaustion behavior when stamina reaches zero.
- Expose stamina events for UI.

### Acceptance Criteria

- Running consumes stamina.
- Stamina recovers over time.
- Player cannot run indefinitely.
- UI or debug output can display stamina changes.

---

## Task 1.7 — Add Basic Camera Motion Feedback

### Description

Add subtle camera effects to improve embodiment without causing discomfort.

### Scope

- Add subtle head bob while moving.
- Add slight sway while turning or moving.
- Add configurable intensity.
- Ensure effects can be disabled.

### Acceptance Criteria

- Camera feedback improves movement feel.
- Effects are subtle and not excessive.
- Effects can be tuned or disabled.
- No motion sickness-inducing behavior is introduced.

---

# Milestone 2 — Interaction and World Objects

## Goal

Allow the player to interact with the environment and create the foundation for exploration-based gameplay.

---

## Task 2.1 — Create Interaction Contracts

### Description

Create the base interaction contracts used by all interactable objects.

### Scope

- Create `IInteractable`.
- Define interaction text or prompt data.
- Define `CanInteract` validation.
- Define `Interact` execution.
- Create supporting interaction context if needed.

### Acceptance Criteria

- Interactable objects can implement a shared contract.
- Interaction logic does not depend directly on UI.
- The interaction contract is inside the correct assembly boundary.

---

## Task 2.2 — Implement Player Interactor

### Description

Create the player-side interactor responsible for detecting interactable objects in front of the camera.

### Scope

- Add raycast or spherecast detection.
- Add interaction distance.
- Detect valid `IInteractable` targets.
- Trigger interaction on input.
- Expose current target changes for UI.

### Acceptance Criteria

- Player can detect interactable objects.
- Player can interact with the detected object.
- Invalid objects are ignored.
- Interaction distance is configurable.

---

## Task 2.3 — Implement Interaction Prompt UI

### Description

Create a minimal UI prompt showing the current available interaction.

### Scope

- Create prompt view.
- Show prompt when an interactable is targeted.
- Hide prompt when no target is available.
- Display interaction text.

### Acceptance Criteria

- Prompt appears when looking at an interactable.
- Prompt disappears when looking away.
- Prompt updates when target changes.
- UI does not execute gameplay logic directly.

---

## Task 2.4 — Implement Door Interaction

### Description

Create a basic door interactable that can open and close.

### Scope

- Create door prefab.
- Implement open and close behavior.
- Add interaction prompt text.
- Add optional locked state placeholder.
- Add placeholder sound event hook.

### Acceptance Criteria

- Player can open and close the door.
- Door state is preserved while scene is active.
- Door interaction uses the shared interaction system.
- Door can later support locked/unlocked states.

---

## Task 2.5 — Implement Pickup Interaction

### Description

Create a simple pickup object that the player can collect.

### Scope

- Create pickup prefab.
- Add item identifier.
- Add collect interaction.
- Hide or destroy object after collection.
- Emit collection event.

### Acceptance Criteria

- Player can collect the pickup.
- Pickup disappears or becomes inactive after collection.
- A collection event is emitted.
- The system is ready to be used by objectives.

---

## Task 2.6 — Implement Note Interaction

### Description

Create a note/document interactable that can display readable text to the player.

### Scope

- Create note data asset or component.
- Create note interactable.
- Create minimal note reading UI.
- Pause or limit gameplay input while reading if needed.
- Emit note collected/read event.

### Acceptance Criteria

- Player can interact with a note.
- Note text is displayed in UI.
- Player can close the note view.
- Note system does not directly control mission logic.

---

# Milestone 3 — Objectives and Mission Flow

## Goal

Create a simple mission system capable of guiding the player through a short playable sequence.

---

## Task 3.1 — Create Objective Base System

### Description

Create the base classes or interfaces for gameplay objectives.

### Scope

- Create objective contract or base class.
- Add objective state: inactive, active, completed, failed.
- Add title and description.
- Add activation and completion methods.
- Add objective events.

### Acceptance Criteria

- Objectives can be activated and completed.
- Objective state transitions are controlled.
- Objective changes can be observed by UI or mission systems.

---

## Task 3.2 — Implement Objective Manager

### Description

Create the manager responsible for controlling the current mission objective sequence.

### Scope

- Register objective list.
- Activate first objective.
- Listen for objective completion.
- Activate next objective.
- Notify UI when objective changes.

### Acceptance Criteria

- A linear objective sequence can run from start to finish.
- Completed objectives activate the next objective.
- Current objective data can be displayed in UI.
- Objective flow can be debugged through logs.

---

## Task 3.3 — Implement Find Item Objective

### Description

Create an objective that completes when the player collects a specific item.

### Scope

- Listen for item collected event.
- Match required item identifier.
- Complete objective when item is collected.

### Acceptance Criteria

- Objective completes when the correct item is collected.
- Objective does not complete for unrelated items.
- Objective can be reused for different item identifiers.

---

## Task 3.4 — Implement Interact Objective

### Description

Create an objective that completes when the player interacts with a specific world object.

### Scope

- Listen for interaction event.
- Match target identifier.
- Complete objective when correct interaction occurs.

### Acceptance Criteria

- Objective completes after interacting with the correct object.
- Wrong interactions do not complete the objective.
- Objective can be reused for doors, switches, machines, or story objects.

---

## Task 3.5 — Implement Reach Area Objective

### Description

Create an objective that completes when the player reaches a specific trigger area.

### Scope

- Create objective trigger volume.
- Detect player entry.
- Complete objective on valid entry.
- Add debug visualization.

### Acceptance Criteria

- Objective completes when the player enters the target area.
- Trigger ignores unrelated objects.
- Area objective can be reused across scenes.

---

## Task 3.6 — Implement Basic Objective HUD

### Description

Create a minimal HUD element showing the current objective.

### Scope

- Create objective text view.
- Update text when objective changes.
- Hide when no objective is active.
- Add optional completion feedback.

### Acceptance Criteria

- Current objective appears on screen.
- Objective text updates correctly.
- Completion feedback is shown or logged.
- UI reads objective data without controlling objective state.

---

## Task 3.7 — Create First Mission Sequence

### Description

Build the first simple mission sequence using the objective system.

### Scope

- Objective 1: Reach area or inspect environment.
- Objective 2: Find item.
- Objective 3: Interact with object.
- Objective 4: Escape or reach final area.

### Acceptance Criteria

- Player can complete the mission sequence from start to finish.
- Objectives update correctly.
- The sequence can be played inside the playground scene.
- The flow exposes issues in interaction, UI, or scene structure.

---

# Milestone 4 — Health, Damage, Death, and Restart

## Goal

Create the minimum failure loop required for a survival/horror gameplay experience.

---

## Task 4.1 — Implement Player Health

### Description

Create a health component for the player with damage, healing, death, and event notifications.

### Scope

- Add max health.
- Add current health.
- Add damage method.
- Add heal method placeholder.
- Add death event.
- Add damage feedback event.

### Acceptance Criteria

- Player can receive damage.
- Health cannot go below zero.
- Death event is triggered when health reaches zero.
- Health values can be displayed or logged.

---

## Task 4.2 — Implement Damage Source

### Description

Create a reusable damage source component for enemies, hazards, and scripted events.

### Scope

- Add damage amount.
- Add cooldown or single-hit option.
- Detect player target.
- Apply damage through health component.

### Acceptance Criteria

- A damage source can damage the player.
- Damage amount is configurable.
- Damage application is not duplicated across systems.
- Damage can later be used by enemies and hazards.

---

## Task 4.3 — Implement Death State Flow

### Description

Connect player death to the application state machine and show a death flow.

### Scope

- Listen to player death event.
- Transition to `DeathState`.
- Disable gameplay input.
- Show death screen placeholder.
- Add restart option.

### Acceptance Criteria

- Player death changes the game state.
- Gameplay input is disabled after death.
- Death screen appears.
- Player can restart from the death screen.

---

## Task 4.4 — Implement Simple Checkpoint System

### Description

Create a minimal checkpoint system for restarting after death.

### Scope

- Store last checkpoint position.
- Store current objective index if needed.
- Respawn player at checkpoint.
- Reset enemy position or scene state minimally.

### Acceptance Criteria

- Player restarts from a controlled checkpoint.
- Restart does not require manually reloading the entire editor scene.
- Objective progress is restored or reset according to current design.
- The system is simple and does not attempt to be a full save system yet.

---

# Milestone 5 — Enemy AI Prototype

## Goal

Create the first functional enemy capable of creating tension through patrol, perception, chase, and attack.

---

## Task 5.1 — Create Enemy Prefab

### Description

Create the base enemy prefab with navigation, detection, animation placeholder, and state machine components.

### Scope

- Create enemy root prefab.
- Add NavMeshAgent.
- Add collider.
- Add placeholder model.
- Add enemy state machine component.
- Add debug visualization support.

### Acceptance Criteria

- Enemy prefab can be placed in the playground scene.
- Enemy can use NavMeshAgent.
- Enemy has configurable movement values.
- Enemy can be reused for prototype encounters.

---

## Task 5.2 — Implement Enemy State Machine

### Description

Create the base AI state machine for the enemy behavior flow.

### Scope

- Create enemy state contract.
- Implement state machine controller.
- Add Idle state.
- Add Patrol state placeholder.
- Add Investigate state placeholder.
- Add Chase state placeholder.
- Add Attack state placeholder.
- Add debug state logging.

### Acceptance Criteria

- Enemy can enter and exit states.
- State changes are logged or visible in debug mode.
- State machine is isolated from UI and mission logic.
- New states can be added without rewriting the entire enemy.

---

## Task 5.3 — Implement Patrol Behavior

### Description

Create patrol behavior using waypoints and wait times.

### Scope

- Create waypoint route data.
- Move enemy between waypoints.
- Add wait time at waypoint.
- Loop route.
- Support patrol pause when state changes.

### Acceptance Criteria

- Enemy patrols between waypoints.
- Enemy waits at waypoints.
- Patrol can be interrupted by detection.
- Patrol resumes or resets after investigation/chase.

---

## Task 5.4 — Implement Vision Perception

### Description

Add basic player detection using distance, angle, and line-of-sight checks.

### Scope

- Add detection radius.
- Add field-of-view angle.
- Add raycast visibility check.
- Detect player target.
- Emit detection events.

### Acceptance Criteria

- Enemy detects the player inside vision range.
- Enemy does not detect player through walls.
- Enemy ignores player outside the vision cone.
- Detection can be debugged visually.

---

## Task 5.5 — Implement Sound Perception

### Description

Add simple sound-based perception so the enemy can investigate noise events.

### Scope

- Create noise event data.
- Allow player footsteps or interactions to emit noise.
- Enemy listens to nearby noise events.
- Enemy transitions to Investigate state.

### Acceptance Criteria

- Enemy reacts to noise events.
- Noise has configurable radius or intensity.
- Enemy moves toward the last heard position.
- Noise perception does not require direct reference to player implementation.

---

## Task 5.6 — Implement Investigate Behavior

### Description

Create investigation behavior where the enemy moves to the last seen or heard position.

### Scope

- Store investigation position.
- Move enemy to that position.
- Search briefly on arrival.
- Return to patrol if nothing is found.
- Transition to chase if player is detected.

### Acceptance Criteria

- Enemy investigates suspicious locations.
- Enemy does not instantly forget the player.
- Enemy returns to patrol after unsuccessful investigation.
- Investigation creates readable tension for the player.

---

## Task 5.7 — Implement Chase Behavior

### Description

Create enemy chase behavior when the player is detected.

### Scope

- Set player as chase target.
- Move toward player using NavMeshAgent.
- Track last known position.
- Lose target after line-of-sight timeout.
- Transition to investigate/search if target is lost.

### Acceptance Criteria

- Enemy chases the player after detection.
- Enemy can lose the player.
- Enemy searches the last known position after losing sight.
- Chase speed is configurable.

---

## Task 5.8 — Implement Attack Behavior

### Description

Create a basic enemy attack behavior that damages the player when close enough.

### Scope

- Add attack range.
- Add attack cooldown.
- Apply damage to player health.
- Add placeholder animation or timing delay.
- Add audio event hook.

### Acceptance Criteria

- Enemy attacks the player in range.
- Enemy damage is applied correctly.
- Attack cooldown prevents constant damage spam.
- Player can die from enemy attacks.

---

# Milestone 6 — Audio and Atmosphere Foundation

## Goal

Create the audio foundation required to make Dead Echo feel immersive even with limited visual complexity.

---

## Task 6.1 — Implement Audio Service

### Description

Create the main audio service responsible for playing SFX, ambience, and music.

### Scope

- Create `IAudioService` contract.
- Implement one-shot SFX playback.
- Implement loop playback.
- Implement ambience playback.
- Add basic AudioSource pooling.
- Add volume categories.

### Acceptance Criteria

- Gameplay systems can request sounds through the audio service.
- AudioSource pooling works for repeated SFX.
- SFX and ambience can be controlled separately.
- Systems do not manually instantiate AudioSources everywhere.

---

## Task 6.2 — Create Audio Event Assets

### Description

Create ScriptableObject-based audio event assets to decouple sound configuration from gameplay code.

### Scope

- Create `AudioEvent` ScriptableObject.
- Support clip list.
- Support volume range.
- Support pitch variation.
- Support playback type.
- Create sample events.

### Acceptance Criteria

- Audio events can be created from the Unity asset menu.
- Gameplay can trigger an audio event without knowing clip details.
- Random clip variation works.
- Volume and pitch variation are configurable.

---

## Task 6.3 — Implement Player Footstep Audio

### Description

Add footstep sounds based on movement state and surface type if available.

### Scope

- Emit footsteps while walking.
- Emit faster/louder footsteps while running.
- Emit quieter footsteps while crouching.
- Add surface type placeholder.
- Add random variation.

### Acceptance Criteria

- Walking produces footstep sounds.
- Running and crouching sound different.
- Footstep timing follows movement speed.
- System can later support multiple surfaces.

---

## Task 6.4 — Implement Door and Interaction Audio Hooks

### Description

Connect world interactions to audio events.

### Scope

- Add door open sound.
- Add door close sound.
- Add pickup sound.
- Add note interaction sound.
- Add failed/locked interaction placeholder.

### Acceptance Criteria

- Interactions produce audio feedback.
- Sounds are configured through audio events.
- Interaction code does not hardcode audio clips.

---

## Task 6.5 — Implement Enemy Audio Hooks

### Description

Add enemy audio feedback for detection, investigation, chase, proximity, and attack.

### Scope

- Add idle or distant sound hook.
- Add investigation sound hook.
- Add detection sound hook.
- Add chase sound hook.
- Add attack sound hook.
- Add proximity breathing or presence sound placeholder.

### Acceptance Criteria

- Enemy emits readable audio feedback.
- Player can perceive danger before seeing the enemy.
- Enemy audio is triggered by AI state changes.
- Audio hooks do not hardcode specific clips.

---

## Task 6.6 — Implement Ambience Zones

### Description

Create ambience zones that change environmental audio based on player location.

### Scope

- Create ambience zone component.
- Detect player entering/exiting zone.
- Crossfade ambience loops.
- Add zone priority placeholder.

### Acceptance Criteria

- Ambience changes between areas.
- Crossfade is smooth.
- Zones can be placed in level scenes.
- Ambience is controlled through audio service.

---

# Milestone 7 — Narrative and Environmental Storytelling

## Goal

Create the minimum narrative systems required to tell story through exploration, documents, audio, and environmental events.

---

## Task 7.1 — Implement Note Data System

### Description

Create the data structure for notes, documents, logs, and environmental text.

### Scope

- Create note data asset.
- Add title.
- Add body text.
- Add optional author/date metadata.
- Add optional unique identifier.

### Acceptance Criteria

- Notes can be created as data assets.
- Notes can be referenced by note interactables.
- Notes are not hardcoded inside UI views.

---

## Task 7.2 — Implement Journal System

### Description

Create a simple journal system that stores discovered notes and allows the player to revisit them.

### Scope

- Store collected note IDs.
- Add journal service or manager.
- Add minimal journal UI.
- Support opening a collected note.

### Acceptance Criteria

- Collected notes are stored during gameplay.
- Player can open the journal.
- Player can reread collected notes.
- Journal does not depend on mission-specific logic.

---

## Task 7.3 — Implement Narrative Trigger System

### Description

Create a flexible trigger system for environmental storytelling events.

### Scope

- Create trigger volume.
- Trigger once or multiple times.
- Emit event on player entry.
- Support actions such as audio playback, light flicker, door change, objective update, or enemy activation through high-level events.

### Acceptance Criteria

- Narrative trigger activates when player enters area.
- Trigger can be configured to run once.
- Trigger can call high-level actions without tightly coupling to all systems.
- Trigger is reusable across scenes.

---

## Task 7.4 — Implement Simple Environmental Event Actions

### Description

Create reusable actions that can be triggered by narrative or gameplay events.

### Scope

- Play audio action.
- Enable/disable GameObject action.
- Flicker light action.
- Open/close door action.
- Activate enemy action.

### Acceptance Criteria

- Multiple actions can be triggered from one event.
- Actions are reusable.
- Actions are configured in the Inspector.
- Actions avoid hardcoded scene-specific scripts where possible.

---

## Task 7.5 — Create First Environmental Storytelling Pass

### Description

Add the first set of environmental storytelling elements to the prototype level.

### Scope

- Add one room with clear narrative context.
- Add visual clues.
- Add one note.
- Add one sound event.
- Add one scripted environmental change.

### Acceptance Criteria

- The environment communicates a small story without dialogue.
- Player can understand that something happened in the location.
- The sequence uses existing interaction, audio, and narrative systems.

---

# Milestone 8 — First Micro Vertical Slice

## Goal

Create a short playable sequence of approximately 2 to 5 minutes that validates the core experience.

---

## Task 8.1 — Build Micro Vertical Slice Graybox

### Description

Create a small graybox level focused on flow, tension, and system validation.

### Scope

- Create starting area.
- Create exploration corridor.
- Create two or three rooms.
- Create objective area.
- Create enemy encounter area.
- Create escape area.

### Acceptance Criteria

- The level can be played from start to finish.
- Navigation is readable.
- The level supports the current objective sequence.
- The level does not require final art.

---

## Task 8.2 — Integrate Mission Flow into Micro Slice

### Description

Connect the objective system to the micro vertical slice level.

### Scope

- Add objective sequence to the level.
- Add item objective.
- Add interaction objective.
- Add escape objective.
- Connect HUD updates.

### Acceptance Criteria

- Player always has a clear current objective.
- Objectives complete correctly.
- The mission sequence can be completed without editor intervention.

---

## Task 8.3 — Integrate Enemy Encounter into Micro Slice

### Description

Place and configure the first enemy encounter inside the micro vertical slice.

### Scope

- Add enemy patrol route.
- Add detection zones or route design.
- Add chase opportunity.
- Add escape route.
- Tune speed and detection values.

### Acceptance Criteria

- Enemy creates tension without being unfair.
- Player can survive through movement and awareness.
- Enemy can kill the player if ignored.
- Encounter can be replayed consistently.

---

## Task 8.4 — Add Audio Atmosphere to Micro Slice

### Description

Add the first audio pass to the micro vertical slice.

### Scope

- Add ambience zones.
- Add footsteps.
- Add door and interaction sounds.
- Add enemy sounds.
- Add one or more narrative sound events.

### Acceptance Criteria

- The micro slice has a clear soundscape.
- Audio supports tension and readability.
- Player receives audio feedback for important actions.

---

## Task 8.5 — Add Death and Restart Flow to Micro Slice

### Description

Ensure the player can fail, die, and restart the micro vertical slice.

### Scope

- Add enemy damage.
- Add death state.
- Add restart button.
- Add checkpoint or reset behavior.

### Acceptance Criteria

- Player can die during the slice.
- Death screen appears correctly.
- Restart returns player to a valid state.
- Restart does not break objectives or enemy behavior.

---

## Task 8.6 — Internal Playtest Micro Slice

### Description

Run an internal playtest of the micro vertical slice and collect issues related to controls, clarity, tension, bugs, and performance.

### Scope

- Play the slice from start to finish multiple times.
- Document bugs.
- Document confusing points.
- Document pacing problems.
- Document performance issues.

### Acceptance Criteria

- At least one playtest report is created.
- Major blockers are identified.
- Required improvements are converted into tasks.
- The slice can be completed by someone other than the developer if possible.

---

# Milestone 9 — Expanded Gameplay Systems

## Goal

Add depth only after the core playable loop is proven.

---

## Task 9.1 — Implement Simple Inventory

### Description

Create a minimal inventory system for key items, notes, and simple usable items.

### Scope

- Store collected item IDs.
- Support key items.
- Support document references.
- Add basic inventory UI placeholder.
- Add item use event placeholder.

### Acceptance Criteria

- Player can collect items into inventory.
- Key items can be checked by locked objects or objectives.
- Inventory remains simple and avoids grid/weight complexity.
- Inventory can be expanded later.

---

## Task 9.2 — Implement Locked Door and Key Flow

### Description

Add a simple locked door system integrated with inventory or objective progression.

### Scope

- Add locked state to doors.
- Add required key ID.
- Check inventory on interaction.
- Show locked feedback.
- Unlock and open when requirements are met.

### Acceptance Criteria

- Locked doors prevent progress until key is collected.
- Player receives feedback when door is locked.
- Door unlocks after the correct item is available.
- System uses existing interaction and inventory logic.

---

## Task 9.3 — Prototype Main Dead Echo Tool

### Description

Prototype a unique player tool based on the Dead Echo concept, such as an audio/echo device that reveals clues, memories, or enemy presence.

### Scope

- Define initial tool behavior.
- Add use input.
- Add cooldown or energy limitation.
- Reveal hidden clue or trigger echo event.
- Add audio and visual feedback.

### Acceptance Criteria

- Player can use the tool during gameplay.
- The tool reveals useful information or affects tension.
- The tool has a limitation to avoid spam.
- The mechanic feels thematically connected to Dead Echo.

---

## Task 9.4 — Implement Simple Puzzle Flow

### Description

Create a basic puzzle using existing interaction, inventory, objective, and narrative systems.

### Scope

- Create puzzle objective.
- Add required interaction sequence.
- Add feedback for wrong/incomplete state.
- Add success event.
- Connect puzzle completion to mission progress.

### Acceptance Criteria

- Player can understand and solve the puzzle through environment clues.
- Puzzle does not require external explanation.
- Puzzle completion changes the world or advances objective.
- Puzzle uses reusable systems instead of custom one-off logic only.

---

## Task 9.5 — Add Second Enemy Behavior Variant

### Description

Add a behavior variation to the enemy or create a second simple enemy type to increase gameplay depth.

### Scope

- Define behavior variation.
- Example: reacts strongly to sound.
- Example: reacts to light.
- Example: blocks route instead of chasing directly.
- Add configuration and test setup.

### Acceptance Criteria

- The new behavior changes how the player acts.
- Behavior is readable and fair.
- The system reuses existing AI foundation.
- The variation can be enabled without duplicating the full enemy implementation.

---

# Milestone 10 — Art, Level Design, and Visual Identity

## Goal

Transform the prototype into a visually coherent and atmospheric experience.

---

## Task 10.1 — Create Main Level Graybox

### Description

Create the graybox for the first real level or demo area.

### Scope

- Define macro layout.
- Add main route.
- Add optional route.
- Add safe area.
- Add tension area.
- Add encounter area.
- Add objective locations.

### Acceptance Criteria

- The level can be played without final art.
- The layout supports the intended pacing.
- Player navigation is readable.
- The level supports enemy and objective systems.

---

## Task 10.2 — Create Modular Environment Kit

### Description

Create the first reusable modular environment kit for building Dead Echo locations efficiently.

### Scope

- Create wall modules.
- Create floor modules.
- Create ceiling modules.
- Create door frames.
- Create windows or blocked openings.
- Create basic props.
- Create decals or damage details.

### Acceptance Criteria

- Rooms can be built using modular pieces.
- Pieces align correctly on grid.
- Visual style is consistent.
- Kit supports fast iteration.

---

## Task 10.3 — First Lighting Pass

### Description

Create the first lighting pass for the demo environment.

### Scope

- Add practical lights.
- Add controlled darkness.
- Add flickering lights where appropriate.
- Add fog or volume effects if useful.
- Configure post-processing.

### Acceptance Criteria

- Lighting improves atmosphere and readability.
- Important paths and interactables are not completely hidden unintentionally.
- Performance remains acceptable.
- Lighting direction supports horror mood.

---

## Task 10.4 — First Prop and Set Dressing Pass

### Description

Add props and environmental storytelling details to make the level feel lived-in and abandoned.

### Scope

- Add furniture.
- Add debris.
- Add story props.
- Add visual clues.
- Add damage and decay details.
- Add blocked paths where needed.

### Acceptance Criteria

- Rooms feel more believable.
- Props support the story and navigation.
- Set dressing does not block gameplay unintentionally.
- Performance remains acceptable.

---

## Task 10.5 — Visual Composition Pass

### Description

Improve visual composition, silhouettes, focal points, and readability in the demo area.

### Scope

- Adjust sightlines.
- Improve silhouettes.
- Add focal lights or contrast.
- Reduce visual noise.
- Guide player attention through environment design.

### Acceptance Criteria

- Player can understand where to look and where to go.
- Important objects stand out without excessive UI markers.
- The scene feels more cinematic without relying on expensive cutscenes.

---

# Milestone 11 — Product Systems

## Goal

Add essential systems expected from a playable PC demo or early build.

---

## Task 11.1 — Implement Save System Foundation

### Description

Create the foundation for saving player progress, settings, collected notes, and checkpoints.

### Scope

- Create save data structure.
- Save checkpoint data.
- Save collected notes.
- Save basic progress flags.
- Add save/load service.

### Acceptance Criteria

- Progress can be saved.
- Progress can be loaded.
- Save data is versioned or structured for future changes.
- Save logic is not directly embedded in UI or individual gameplay objects.

---

## Task 11.2 — Implement Settings Menu

### Description

Create basic settings for audio, video, and controls.

### Scope

- Master volume.
- SFX volume.
- Music/ambience volume.
- Mouse sensitivity.
- Fullscreen toggle.
- Resolution placeholder if needed.
- Invert Y option.

### Acceptance Criteria

- Settings can be changed in game.
- Settings persist between sessions.
- Audio settings affect audio categories.
- Mouse sensitivity affects camera look.

---

## Task 11.3 — Implement Pause Menu

### Description

Create a robust pause system integrated with the app state machine and input service.

### Scope

- Pause input action.
- Pause state or overlay.
- Resume option.
- Settings option.
- Return to main menu option.
- Block gameplay input while paused.

### Acceptance Criteria

- Player can pause and resume.
- Gameplay input is blocked while paused.
- UI input still works while paused.
- Returning to menu works without broken state.

---

## Task 11.4 — Implement Main Menu Flow

### Description

Create the initial main menu flow for starting the game, opening settings, and exiting.

### Scope

- Start game button.
- Continue button placeholder.
- Settings button.
- Exit button.
- Transition to loading/gameplay.

### Acceptance Criteria

- Main menu appears after boot.
- Start game loads the gameplay scene.
- Settings can be opened from main menu.
- Exit works in build or logs correctly in editor.

---

# Milestone 12 — Optimization and Stability

## Goal

Improve stability, performance, and memory behavior without over-optimizing too early.

---

## Task 12.1 — Create Performance Baseline

### Description

Measure the current performance of the vertical slice and define initial baseline metrics.

### Scope

- Measure FPS.
- Measure CPU spikes.
- Measure GPU cost.
- Measure memory usage.
- Measure GC allocations.
- Measure loading time.

### Acceptance Criteria

- A performance baseline document exists.
- Main bottlenecks are identified.
- Performance issues are converted into tasks.
- Future builds can be compared against the baseline.

---

## Task 12.2 — Reduce GC Allocations in Core Gameplay

### Description

Identify and reduce avoidable garbage allocations in frequent gameplay systems.

### Scope

- Profile player controller.
- Profile interaction detection.
- Profile AI update loop.
- Profile UI updates.
- Remove unnecessary per-frame allocations.

### Acceptance Criteria

- Major recurring allocations are identified.
- High-frequency systems generate minimal avoidable GC.
- Performance improvements are verified in Profiler.

---

## Task 12.3 — Implement Pooling for Repeated Temporary Objects

### Description

Add pooling where it clearly improves performance and memory behavior.

### Scope

- Pool AudioSources.
- Pool repeated VFX if applicable.
- Pool temporary UI elements if applicable.
- Pool decals or repeated spawned objects if applicable.

### Acceptance Criteria

- Pooling is used only where justified.
- Repeated spawning no longer creates unnecessary spikes.
- Pooled objects reset state correctly.
- Pooling does not add unnecessary complexity to unrelated systems.

---

## Task 12.4 — Prepare Addressables Strategy

### Description

Define and prepare Addressables usage for heavier content and future streaming needs.

### Scope

- Identify heavy assets.
- Define Addressables groups.
- Define labels.
- Create loading/unloading strategy.
- Avoid converting everything too early.

### Acceptance Criteria

- Addressables strategy is documented.
- Heavy assets are identified.
- Initial groups and labels are created if needed.
- Loading strategy supports future content expansion.

---

# Recommended Sprint Order

## Sprint 1 — Foundation

### Tasks

- Task 0.1 — Create Unity Project and Configure Base Settings
- Task 0.2 — Create Assembly Definitions
- Task 0.3 — Install and Configure VContainer
- Task 0.4 — Implement Initial App State Machine
- Task 0.5 — Implement Scene Loading Service
- Task 0.6 — Implement Input Service
- Task 0.7 — Create Technical Playground Scene

### Sprint Result

The game opens, passes through the initial states, and loads a playable empty test scene.

---

## Sprint 2 — Player Controller

### Tasks

- Task 1.1 — Create Player Prefab
- Task 1.2 — Implement Basic Movement
- Task 1.3 — Implement Camera Look
- Task 1.4 — Implement Running
- Task 1.5 — Implement Crouch
- Task 1.6 — Implement Stamina System
- Task 1.7 — Add Basic Camera Motion Feedback

### Sprint Result

The player can move around the scene with an acceptable first-person game feel.

---

## Sprint 3 — Interaction

### Tasks

- Task 2.1 — Create Interaction Contracts
- Task 2.2 — Implement Player Interactor
- Task 2.3 — Implement Interaction Prompt UI
- Task 2.4 — Implement Door Interaction
- Task 2.5 — Implement Pickup Interaction
- Task 2.6 — Implement Note Interaction

### Sprint Result

The player can inspect and interact with basic world objects.

---

## Sprint 4 — Objectives and Failure Loop

### Tasks

- Task 3.1 — Create Objective Base System
- Task 3.2 — Implement Objective Manager
- Task 3.3 — Implement Find Item Objective
- Task 3.4 — Implement Interact Objective
- Task 3.5 — Implement Reach Area Objective
- Task 3.6 — Implement Basic Objective HUD
- Task 3.7 — Create First Mission Sequence
- Task 4.1 — Implement Player Health
- Task 4.2 — Implement Damage Source
- Task 4.3 — Implement Death State Flow
- Task 4.4 — Implement Simple Checkpoint System

### Sprint Result

The player can follow objectives, fail, die, and restart.

---

## Sprint 5 — Enemy Prototype

### Tasks

- Task 5.1 — Create Enemy Prefab
- Task 5.2 — Implement Enemy State Machine
- Task 5.3 — Implement Patrol Behavior
- Task 5.4 — Implement Vision Perception
- Task 5.5 — Implement Sound Perception
- Task 5.6 — Implement Investigate Behavior
- Task 5.7 — Implement Chase Behavior
- Task 5.8 — Implement Attack Behavior

### Sprint Result

The game has a real threat capable of patrolling, detecting, chasing, attacking, and killing the player.

---

## Sprint 6 — Audio and Atmosphere

### Tasks

- Task 6.1 — Implement Audio Service
- Task 6.2 — Create Audio Event Assets
- Task 6.3 — Implement Player Footstep Audio
- Task 6.4 — Implement Door and Interaction Audio Hooks
- Task 6.5 — Implement Enemy Audio Hooks
- Task 6.6 — Implement Ambience Zones

### Sprint Result

The prototype starts to feel immersive through footsteps, ambience, interaction sounds, and enemy audio cues.

---

## Sprint 7 — Narrative Prototype

### Tasks

- Task 7.1 — Implement Note Data System
- Task 7.2 — Implement Journal System
- Task 7.3 — Implement Narrative Trigger System
- Task 7.4 — Implement Simple Environmental Event Actions
- Task 7.5 — Create First Environmental Storytelling Pass

### Sprint Result

The environment begins to communicate story through notes, triggers, audio, and world details.

---

## Sprint 8 — Micro Vertical Slice

### Tasks

- Task 8.1 — Build Micro Vertical Slice Graybox
- Task 8.2 — Integrate Mission Flow into Micro Slice
- Task 8.3 — Integrate Enemy Encounter into Micro Slice
- Task 8.4 — Add Audio Atmosphere to Micro Slice
- Task 8.5 — Add Death and Restart Flow to Micro Slice
- Task 8.6 — Internal Playtest Micro Slice

### Sprint Result

A short playable experience of approximately 2 to 5 minutes exists and validates the core Dead Echo experience.

---

## Sprint 9 — Gameplay Expansion

### Tasks

- Task 9.1 — Implement Simple Inventory
- Task 9.2 — Implement Locked Door and Key Flow
- Task 9.3 — Prototype Main Dead Echo Tool
- Task 9.4 — Implement Simple Puzzle Flow
- Task 9.5 — Add Second Enemy Behavior Variant

### Sprint Result

The game gains more depth through inventory, locked progression, puzzle flow, and the first unique Dead Echo mechanic.

---

## Sprint 10 — Art and Level Production

### Tasks

- Task 10.1 — Create Main Level Graybox
- Task 10.2 — Create Modular Environment Kit
- Task 10.3 — First Lighting Pass
- Task 10.4 — First Prop and Set Dressing Pass
- Task 10.5 — Visual Composition Pass

### Sprint Result

The game starts moving from prototype to an atmospheric production-level demo.

---

## Sprint 11 — Product Systems

### Tasks

- Task 11.1 — Implement Save System Foundation
- Task 11.2 — Implement Settings Menu
- Task 11.3 — Implement Pause Menu
- Task 11.4 — Implement Main Menu Flow

### Sprint Result

The demo becomes closer to a real product build with menu, settings, save, and pause support.

---

## Sprint 12 — Optimization and Stability

### Tasks

- Task 12.1 — Create Performance Baseline
- Task 12.2 — Reduce GC Allocations in Core Gameplay
- Task 12.3 — Implement Pooling for Repeated Temporary Objects
- Task 12.4 — Prepare Addressables Strategy

### Sprint Result

The project becomes more stable, measurable, and ready for a larger demo production phase.

---

# First Critical Target

The first critical target is:

```text
Create a playable micro vertical slice with player movement, interaction, objectives, one enemy, audio atmosphere, death/restart, and a short mission flow.
```

Recommended minimum experience:

```text
Start in a small abandoned location.
Explore two or three rooms.
Find a clue or key item.
Trigger a threat.
Escape or reach a safe area.
Die and restart if caught.
```

---

# What Should Not Be Prioritized Early

Avoid spending major time early on:

```text
Complex cinematics.
Advanced facial animation.
Multiple enemy types.
Large inventory systems.
Weapon variety.
Branching dialogue.
Large open levels.
ECS/Burst optimization.
Full save/load complexity.
Final art before gameplay validation.
```

These can be added later after the core loop proves itself.

---

# Final Recommendation

The best development order for Dead Echo is:

```text
Foundation
Player
Interaction
Objectives
Enemy
Audio
Narrative
Micro Vertical Slice
Expansion
Production Art
Product Systems
Optimization
```

The project should be judged first by this question:

```text
Can someone play a short version of Dead Echo and feel tension, curiosity, and fear within a few minutes?
```

If the answer is yes, the project is moving in the right direction.
