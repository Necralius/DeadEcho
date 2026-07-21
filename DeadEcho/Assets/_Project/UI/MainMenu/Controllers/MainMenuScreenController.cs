using System;
using Project.Core.Services;
using Project.UI.Navigation;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.UI.MainMenu.Controllers
{
    public sealed class MainMenuScreenController : IUiScreenController, IUiNavigationRequestSource
    {
        private const string MainMenuTemplatePath = "UI/MainMenu/MainMenu";
        private const string ThemePath = "UI/Shared/MainMenuTheme";

        private readonly IModalService _modalService;
        private readonly ISaveGameQuery _saveGameQuery;
        private readonly INewGameService _newGameService;
        private readonly IApplicationQuitService _quitService;
        private readonly Button _newGameButton;
        private readonly Button _loadGameButton;
        private readonly Button _settingsButton;
        private readonly Button _creditsButton;
        private readonly Button _extrasButton;
        private readonly Button _quitButton;

        public MainMenuScreenController(
            IModalService modalService,
            ISaveGameQuery saveGameQuery,
            INewGameService newGameService,
            IApplicationQuitService quitService)
        {
            _modalService = modalService ?? throw new ArgumentNullException(nameof(modalService));
            _saveGameQuery = saveGameQuery ?? throw new ArgumentNullException(nameof(saveGameQuery));
            _newGameService = newGameService ?? throw new ArgumentNullException(nameof(newGameService));
            _quitService = quitService ?? throw new ArgumentNullException(nameof(quitService));

            Root = BuildRoot();
            _newGameButton = Require<Button>("new-game-button");
            _loadGameButton = Require<Button>("load-game-button");
            _settingsButton = Require<Button>("settings-button");
            _creditsButton = Require<Button>("credits-button");
            _extrasButton = Require<Button>("extras-button");
            _quitButton = Require<Button>("quit-button");
            DefaultFocus = _newGameButton;

            _newGameButton.clicked += OnNewGameClicked;
            _loadGameButton.clicked += OnLoadGameClicked;
            _settingsButton.clicked += OnSettingsClicked;
            _creditsButton.clicked += OnCreditsClicked;
            _extrasButton.clicked += OnExtrasClicked;
            _quitButton.clicked += OnQuitClicked;
        }

        public event Action<UiScreenId> ShowRequested;
        public event Action<UiScreenId> ReplaceRequested;
        public event Action BackRequested { add { } remove { } }

        public UiScreenId ScreenId => UiScreenId.MainMenu;
        public VisualElement Root { get; }
        public VisualElement DefaultFocus { get; private set; }

        public void Open()
        {
            Root.style.display = DisplayStyle.Flex;
            Root.RemoveFromClassList("screen-hidden");
            RefreshLoadGameState();
            _quitButton.style.display = _quitService.IsQuitAvailable ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public void Close()
        {
            Root.AddToClassList("screen-hidden");
            Root.RemoveFromHierarchy();
        }

        public void Dispose()
        {
            _newGameButton.clicked -= OnNewGameClicked;
            _loadGameButton.clicked -= OnLoadGameClicked;
            _settingsButton.clicked -= OnSettingsClicked;
            _creditsButton.clicked -= OnCreditsClicked;
            _extrasButton.clicked -= OnExtrasClicked;
            _quitButton.clicked -= OnQuitClicked;
            Close();
        }

        private VisualElement BuildRoot()
        {
            VisualElement root = LoadTemplate() ?? BuildFallbackRoot();
            root.AddToClassList("main-menu-screen");
            root.pickingMode = PickingMode.Position;
            ApplyTheme(root);
            SetVersionText(root);
            return root;
        }

        private static VisualElement LoadTemplate()
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>(MainMenuTemplatePath);
            TemplateContainer container = template?.CloneTree();
            VisualElement screen = container?.Q<VisualElement>("main-menu-screen");
            screen?.RemoveFromHierarchy();
            return screen;
        }

        private static VisualElement BuildFallbackRoot()
        {
            var root = new VisualElement { name = "main-menu-screen" };
            root.AddToClassList("ui-screen");

            var content = new VisualElement { name = "main-menu-content" };
            content.AddToClassList("main-menu-content");
            root.Add(content);

            var title = new Label("Dead Echo") { name = "main-menu-title" };
            title.AddToClassList("ui-title");
            title.AddToClassList("main-menu-title");
            content.Add(title);

            var subtitle = new Label("A descent into the halls below silence.") { name = "main-menu-subtitle" };
            subtitle.AddToClassList("ui-subtitle");
            subtitle.AddToClassList("main-menu-subtitle");
            content.Add(subtitle);

            var actions = new VisualElement { name = "main-menu-actions" };
            actions.AddToClassList("main-menu-actions");
            content.Add(actions);

            actions.Add(CreateMenuButton("new-game-button", "New Game", "button-primary"));
            actions.Add(CreateMenuButton("load-game-button", "Load Game", "button-secondary"));
            actions.Add(CreateMenuButton("settings-button", "Settings", "button-secondary"));
            actions.Add(CreateMenuButton("credits-button", "Credits", "button-secondary"));
            actions.Add(CreateMenuButton("extras-button", "Extras", "button-secondary"));
            actions.Add(CreateMenuButton("quit-button", "Quit", "button-danger"));

            var footer = new VisualElement { name = "main-menu-footer" };
            footer.AddToClassList("main-menu-footer");
            var copyright = new Label("Dead Echo Studio") { name = "copyright-label" };
            copyright.AddToClassList("footer-label");
            var version = new Label { name = "version-label" };
            version.AddToClassList("footer-label");
            footer.Add(copyright);
            footer.Add(version);
            root.Add(footer);
            return root;
        }

        private static Button CreateMenuButton(string name, string text, string className)
        {
            var button = new Button { name = name, text = text };
            button.AddToClassList(className);
            button.AddToClassList("menu-button");
            return button;
        }

        private static void ApplyTheme(VisualElement root)
        {
            StyleSheet theme = Resources.Load<StyleSheet>(ThemePath);
            if (theme != null)
                root.styleSheets.Add(theme);
        }

        private static void SetVersionText(VisualElement root)
        {
            Label version = root.Q<Label>("version-label");
            if (version != null)
                version.text = $"v{Application.version}";
        }

        private T Require<T>(string name) where T : VisualElement
        {
            T element = Root.Q<T>(name);
            if (element == null)
                throw new InvalidOperationException($"Main menu UXML is missing '{name}'.");

            return element;
        }

        private void RefreshLoadGameState()
        {
            bool hasSave = _saveGameQuery.HasAnySave();
            _loadGameButton.SetEnabled(hasSave);
            _loadGameButton.tooltip = hasSave ? "Load saved game" : "No save data found";
            _loadGameButton.EnableInClassList("button-disabled", !hasSave);
        }

        private async void OnNewGameClicked()
        {
            _newGameButton.SetEnabled(false);
            try
            {
                if (_saveGameQuery.HasAnySave())
                {
                    bool confirmed = await _modalService.ConfirmAsync(new ConfirmationModalRequest(
                        "Start New Game",
                        "Existing progress may be overwritten. Continue?",
                        "Start",
                        "Cancel",
                        ModalVariant.Warning));

                    if (!confirmed)
                        return;
                }

                await _newGameService.StartNewGameAsync();
                ReplaceRequested?.Invoke(UiScreenId.NewGame);
            }
            catch (Exception exception)
            {
                await _modalService.ErrorAsync("New Game Failed", exception.Message);
            }
            finally
            {
                _newGameButton.SetEnabled(true);
            }
        }

        private void OnLoadGameClicked()
        {
            if (!_saveGameQuery.HasAnySave())
                return;

            ShowRequested?.Invoke(UiScreenId.LoadGame);
        }

        private void OnSettingsClicked()
        {
            ShowRequested?.Invoke(UiScreenId.Settings);
        }

        private void OnCreditsClicked()
        {
            ShowRequested?.Invoke(UiScreenId.Credits);
        }

        private void OnExtrasClicked()
        {
            ShowRequested?.Invoke(UiScreenId.Extras);
        }

        private async void OnQuitClicked()
        {
            bool confirmed = await _modalService.ConfirmAsync(
                new ConfirmationModalRequest(
                    "Quit Dead Echo",
                    "Leave the menu and close the game?",
                    "Quit",
                    "Cancel",
                    ModalVariant.Danger));

            if (!confirmed)
                return;

            _quitService.Quit();
        }
    }
}
