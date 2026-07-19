# Dependency Registration

`ProjectLifetimeScope` is the root composition point for global services. Register application-wide services here when they are safe to share across scenes.

Scene-specific services should be registered in `SceneLifetimeScope` instances and parented to the root scope. Avoid registering every `MonoBehaviour` automatically; register only the components that represent intentional dependencies.

Current placeholders:

- `PlaceholderInputService`
- `PlaceholderAudioService`
- `PlaceholderUiService`

Replace placeholders with production implementations behind the same Core interfaces as each system becomes real.
