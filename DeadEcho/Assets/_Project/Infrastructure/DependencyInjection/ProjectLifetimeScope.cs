using Project.Audio.Services;
using Project.Core.Services;
using Project.Core.StateMachine;
using Project.Infrastructure.Input;
using Project.Infrastructure.SceneLoading;
using Project.UI.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VContainer.Unity;

namespace Project.Infrastructure.DependencyInjection
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private InputActionAsset inputActions;

        protected override void Configure(IContainerBuilder builder)
        {
            InputActionAsset inputActionAsset = inputActions != null
                ? inputActions
                : DeadEchoInputActionsFactory.CreateDefault();

            builder.RegisterInstance(inputActionAsset);

            builder.Register<GameEventBus>(Lifetime.Singleton).As<IGameEventBus>();
            builder.Register<UnitySceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<UnityInputService>(Lifetime.Singleton).As<IInputService>();
            builder.Register<PlaceholderAudioService>(Lifetime.Singleton).As<IAudioService>();
            builder.Register<PlaceholderUiService>(Lifetime.Singleton).As<IUiService>();
            builder.Register(resolver => AppStateMachineFactory.CreateDefault(resolver.Resolve<IInputService>()), Lifetime.Singleton);

            builder.RegisterEntryPoint<AppStateMachineStartup>(Lifetime.Singleton);
        }
    }
}
