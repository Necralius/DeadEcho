# Scene Transition Analysis

## Current Flow

Dead Echo currently starts from `Assets/_Project/Scenes/Menu.unity` during development. The scene contains `ProjectLifetimeScope`, which is the main VContainer composition root for global services, UI Toolkit menu controllers, modal services, settings services, input, and the application state machine.

`ProjectLifetimeScope` creates or receives a `UiRoot` backed by a `UIDocument`. The existing menu, modal, settings, load game, credits, and extras screens are routed through `IUiService`, `UiService`, `UiScreenCatalog`, and `ModalService`.

The application state machine is created by `AppStateMachineFactory`. `BootState` transitions to `MainMenuState`, and `MainMenuState` disables gameplay input, enables UI input, and shows `UiScreenId.MainMenu`.

There is an existing `ISceneLoader` contract and `UnitySceneLoader` implementation. `UnitySceneLoader` calls `SceneManager.LoadSceneAsync(sceneName)` directly in single mode and only exposes `IsLoading` and raw `AsyncOperation.progress`. It does not show a loading UI, does not control scene activation, does not warm up the target scene, and does not preserve persistent systems through an additive transition.

`TemporaryNewGameService` and `TemporaryLoadGameService` currently only log requests. They do not start a real gameplay scene yet.

## Build Settings

`ProjectSettings/EditorBuildSettings.asset` currently contains only:

- `Assets/OutdoorsScene.unity`, disabled.

`Assets/_Project/Scenes/Menu.unity` exists but is not registered in Build Settings. Any production transition service that validates scenes by Build Settings will reject both the menu scene and gameplay scenes until they are added and enabled.

## Existing Scene and LifetimeScope Objects

- Persistent/root composition: `Assets/_Project/Infrastructure/DependencyInjection/ProjectLifetimeScope.cs`
- Scene scope placeholder: `Assets/_Project/Infrastructure/DependencyInjection/SceneLifetimeScope.cs`
- UI root: `Assets/_Project/UI/Runtime/UiRoot.cs`
- Current scene loader: `Assets/_Project/Infrastructure/SceneLoading/UnitySceneLoader.cs`
- Menu scene: `Assets/_Project/Scenes/Menu.unity`

`ProjectLifetimeScope` is the correct place to register application-wide transition services. `SceneLifetimeScope` should be used by target scenes for scene-local services and scene initializers when gameplay scenes are introduced.

## Problems Found

- `UnitySceneLoader` calls `SceneManager` directly and bypasses UI Toolkit, readiness, payload, and gameplay gating.
- Scene loading is single-mode, so persistent UI and services can be destroyed if they are not explicitly preserved by scene layout.
- No central transition state machine exists.
- No loading screen exists outside the scene being unloaded.
- Scene activation cannot be controlled.
- No target scene initialization contract exists.
- No warmup/readiness contract exists.
- No payload handoff exists for new game, load game, spawn point, or save id.
- Build Settings are incomplete for validated scene transitions.
- Existing menu action services are placeholders, so New Game and Load Game cannot yet perform real scene transitions without target scene configuration.

## Recommended Architecture

Use the existing `ProjectLifetimeScope` as the persistent composition root. Register a singleton `ISceneTransitionService` there. Keep the loading screen attached to the persistent `UiRoot` so it survives additive scene loading and unloading.

Scene transitions should load target scenes additively with `allowSceneActivation = false`, display progress through `ISceneLoadingView`, activate only when the transition service reaches the activation phase, initialize/warm up the target scene through explicit contracts, set the target as active, unload the previous content scene, then close the loading screen.

The service should be the only production code path that calls `SceneManager` for menu/gameplay transitions. Existing callers should depend on `ISceneTransitionService` or domain services that wrap it.

## Mermaid

```mermaid
flowchart TD
    Menu[Main Menu UI] --> NewGame[INewGameService]
    Load[Load Game UI] --> LoadService[ILoadGameService]
    NewGame --> Transition[ISceneTransitionService]
    LoadService --> Transition
    Transition --> Gate[ISceneGameplayGate]
    Transition --> LoadingView[ISceneLoadingView]
    Transition --> Payload[IScenePayloadStore]
    Transition --> SceneAPI[Unity Scene API Adapter]
    SceneAPI --> TargetScene[Target Scene Additive Load]
    TargetScene --> SceneScope[SceneLifetimeScope]
    SceneScope --> Readiness[ISceneReadinessService]
    Readiness --> Initializers[Scene Initializers / Warmup]
    Transition --> ActiveScene[Set Active Scene]
    Transition --> Unload[Unload Previous Scene]
    Transition --> Complete[Release Gameplay and Hide Loading]
```

## Classes To Create

- `ISceneTransitionService`
- `SceneTransitionService`
- `SceneTransitionRequest`
- `SceneTransitionResult`
- `SceneTransitionStatus`
- `SceneTransitionMode`
- `SceneTransitionProgress`
- `SceneTransitionError`
- `ISceneLoadingView`
- `ISceneReadinessService`
- `SceneReadinessService`
- `ISceneGameplayGate`
- `SceneGameplayGate`
- `IScenePayloadStore`
- `ScenePayloadStore`
- `ScenePayload`
- Unity scene API adapter for validation/loading/activation/unloading
- UI Toolkit loading view

## Classes To Alter

- `ProjectLifetimeScope`: register transition services and the loading view.
- `UnitySceneLoader`: route legacy `ISceneLoader` calls through the transition service or deprecate it.
- `TemporaryNewGameService`: call `ISceneTransitionService` once a target scene name is configured.
- `TemporaryLoadGameService`: call `ISceneTransitionService` with a save payload once a target scene name is configured.
- `UiRoot`: expose a persistent loading layer above screens and modals.

## Full Transition Flow

1. Reject if another transition is active.
2. Validate the target scene against enabled Build Settings scenes.
3. Block gameplay and UI interaction below the loading screen.
4. Store immutable payload if provided.
5. Show the loading screen.
6. Start additive `LoadSceneAsync`.
7. Keep `allowSceneActivation = false` until loading reaches the pre-activation threshold.
8. Activate the scene.
9. Resolve target scene readiness through explicit scene registrations.
10. Initialize scene systems.
11. Warm up systems that need one or more frames.
12. Set target scene as active if requested.
13. Unload the previous content scene if requested.
14. Clear consumed payload.
15. Hide the loading screen.
16. Release gameplay and UI gate.
17. Return a result with status and final state.

## Warmup Participants

Current project code does not yet expose gameplay systems that need transition warmup. Future target scenes should register explicit participants for:

- Player spawn/session initialization.
- Save data restore.
- Input map selection.
- Camera setup.
- Audio snapshot/music setup.
- AI/navmesh preparation.
- Inventory/session data application.
- HUD binding.

## Risks

- Build Settings must be corrected in Unity before real scene transitions can succeed.
- Target gameplay scenes need `SceneLifetimeScope` and initializer registrations before warmup can do useful work.
- Legacy systems with `Start`, `Awake`, or `OnEnable` gameplay side effects may start before the transition releases gameplay unless they check the gameplay gate or are moved behind initializers.
- Scene names in placeholder services must be replaced with configured constants or ScriptableObject configuration before production.
- Unity Test Runner validation may require running inside the Editor, not only `dotnet build`.

## Suggested Implementation Order

1. Add contracts and transition states in `Project.Core`.
2. Add payload store, gameplay gate, readiness service, Unity scene API adapter, and transition service in `Project.Infrastructure`.
3. Add UI Toolkit loading layer and loading view in `Project.UI`.
4. Register everything in `ProjectLifetimeScope`.
5. Route legacy `ISceneLoader`, New Game, and Load Game through transition service.
6. Add EditMode unit tests with fake scene APIs and fake loading view.
7. Add enabled scenes to Build Settings in Unity.
8. Introduce real scene initializers in gameplay scenes.
