using Project.Audio.Services;
using Project.Core.Services;
using Project.Core.StateMachine;
using Project.Infrastructure.Input;
using Project.Infrastructure.SaveGames;
using Project.Infrastructure.SceneLoading;
using Project.Infrastructure.SceneTransitions;
using Project.Infrastructure.Settings;
using Project.Infrastructure.UI;
using Project.UI.MainMenu.Controllers;
using Project.UI.Modals;
using Project.UI.Navigation;
using Project.UI.Runtime;
using Project.UI.SceneTransitions;
using Project.UI.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using VContainer;
using VContainer.Unity;

namespace Project.Infrastructure.DependencyInjection
{
    public sealed class ProjectLifetimeScope : LifetimeScope
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private UiRoot uiRoot;

        protected override void Configure(IContainerBuilder builder)
        {
            InputActionAsset inputActionAsset = inputActions != null
                ? inputActions
                : DeadEchoInputActionsFactory.CreateDefault();
            IUiRoot root = uiRoot != null ? uiRoot : CreateRuntimeUiRoot();

            builder.RegisterInstance(inputActionAsset);
            builder.RegisterInstance(root);

            builder.Register<GameEventBus>(Lifetime.Singleton).As<IGameEventBus>();
            builder.Register<SceneTransitionProgressWeights>(Lifetime.Singleton);
            builder.Register<ScenePayloadStore>(Lifetime.Singleton).As<IScenePayloadStore>();
            builder.Register<SceneGameplayGate>(Lifetime.Singleton).As<ISceneGameplayGate>();
            builder.Register<UnitySceneOperations>(Lifetime.Singleton).As<IUnitySceneOperations>();
            builder.Register<SceneScopeReadinessProvider>(Lifetime.Singleton).As<ISceneScopeReadinessProvider>();
            builder.Register<SceneReadinessService>(Lifetime.Singleton).As<ISceneReadinessService>();
            builder.Register<SceneLoadingView>(Lifetime.Singleton).As<ISceneLoadingView>();
            builder.Register<SceneTransitionService>(Lifetime.Singleton).As<ISceneTransitionService>();
            builder.Register<UnitySceneLoader>(Lifetime.Singleton).As<ISceneLoader>();
            builder.Register<UnityInputService>(Lifetime.Singleton).As<IInputService>();
            builder.Register<PlaceholderAudioService>(Lifetime.Singleton).As<IAudioService>();
            builder.Register<PlayerPrefsSettingsStorage>(Lifetime.Singleton).As<ISettingsStorage>();
            builder.Register(
                    resolver => new UnityAudioSettingsService(resolver.Resolve<ISettingsStorage>()),
                    Lifetime.Singleton)
                .As<IAudioSettingsService>();
            builder.Register<UnityGraphicsSettingsService>(Lifetime.Singleton).As<IGraphicsSettingsService>();
            builder.Register<GraphicsRevertService>(Lifetime.Singleton).As<IGraphicsRevertService>();
            builder.Register<UnityControlsSettingsService>(Lifetime.Singleton).As<IControlsSettingsService>();
            builder.Register<NoSaveGameQuery>(Lifetime.Singleton).As<ISaveGameQuery>();
            builder.Register<TemporaryNewGameFlow>(Lifetime.Singleton).As<INewGameFlow>();
            builder.Register<TemporaryNewGameService>(Lifetime.Singleton).As<INewGameService>();
            builder.Register<TemporaryLoadGameService>(Lifetime.Singleton).As<ILoadGameService>();
            builder.Register<TemporaryDeleteSaveService>(Lifetime.Singleton).As<IDeleteSaveService>();
            builder.Register<UnityApplicationQuitService>(Lifetime.Singleton).As<IApplicationQuitService>();
            builder.Register<StaticCreditsContentProvider>(Lifetime.Singleton).As<ICreditsContentProvider>();
            builder.Register<UiNavigationStack>(Lifetime.Singleton);
            builder.Register<MainMenuScreenController>(Lifetime.Transient);
            builder.Register<LoadGameScreenController>(Lifetime.Transient);
            builder.Register<SettingsScreenController>(Lifetime.Transient);
            builder.Register<CreditsScreenController>(Lifetime.Transient);
            builder.Register<ExtrasScreenController>(Lifetime.Transient);
            builder.RegisterFactory<MainMenuScreenController>(
                resolver => () => resolver.Resolve<MainMenuScreenController>(),
                Lifetime.Singleton);
            builder.RegisterFactory<LoadGameScreenController>(
                resolver => () => resolver.Resolve<LoadGameScreenController>(),
                Lifetime.Singleton);
            builder.RegisterFactory<SettingsScreenController>(
                resolver => () => resolver.Resolve<SettingsScreenController>(),
                Lifetime.Singleton);
            builder.RegisterFactory<CreditsScreenController>(
                resolver => () => resolver.Resolve<CreditsScreenController>(),
                Lifetime.Singleton);
            builder.RegisterFactory<ExtrasScreenController>(
                resolver => () => resolver.Resolve<ExtrasScreenController>(),
                Lifetime.Singleton);
            builder.RegisterFactory<UiScreenId, string, TemporaryMenuScreenController>(
                _ => (screenId, title) => new TemporaryMenuScreenController(screenId, title),
                Lifetime.Singleton);
            builder.Register<UiScreenCatalog>(Lifetime.Singleton);
            builder.Register<ModalService>(Lifetime.Singleton).AsSelf().As<IModalService>();
            builder.Register<UiService>(Lifetime.Singleton).AsSelf().As<IUiService>();
            builder.Register(
                resolver => AppStateMachineFactory.CreateDefault(
                    resolver.Resolve<IInputService>(),
                    resolver.Resolve<IUiService>()),
                Lifetime.Singleton);

            builder.RegisterEntryPoint<AppStateMachineStartup>(Lifetime.Singleton);
        }

        private static UiRoot CreateRuntimeUiRoot()
        {
            var rootObject = new GameObject("UiRoot");
            rootObject.AddComponent<UIDocument>();
            var root = rootObject.AddComponent<UiRoot>();
            root.Initialize();
            return root;
        }
    }
}
