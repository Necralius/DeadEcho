# Dead Echo Module Layout

`Assets/_Project` contains the runtime, editor, and test code organized by ownership boundary. Assemblies should only reference lower-level contracts or modules explicitly allowed by the architecture.

## Modules

- `Core`: Shared contracts, events, utilities, state abstractions, and code that must not depend on higher-level game systems.
- `Gameplay`: Player-facing gameplay systems such as movement, interaction, health, inventory, objectives, and weapons. Depends on `Project.Core`.
- `AI`: Enemy behavior, perception, decision logic, and AI-specific runtime systems. Depends on `Project.Core` and may use minimal `Project.Gameplay` contracts when needed.
- `Audio`: Audio services, audio events, emitters, ambience, and playback coordination. Depends on `Project.Core`.
- `UI`: Runtime UI views, presenters, HUD, menus, and UI bindings. Depends on `Project.Core`; add `Project.Gameplay` only when a concrete gameplay contract is required.
- `Narrative`: Notes, narrative triggers, story events, and scripted narrative flow. Depends on `Project.Core`.
- `Infrastructure`: Technical services such as scene loading, save data, configuration, persistence, and external integrations. Depends on `Project.Core`.
- `Editor`: Unity Editor-only tools, inspectors, validators, and import helpers. This assembly is restricted to the Editor platform.
- `Tests`: EditMode and PlayMode tests. References should stay limited to the module under test plus required test assemblies.

## Dependency Rules

`Project.Core` is the foundation and must remain free of dependencies on gameplay, UI, AI, audio, narrative, or infrastructure implementations. Higher-level modules should communicate through contracts, events, or data types owned by `Core` to avoid circular references.
