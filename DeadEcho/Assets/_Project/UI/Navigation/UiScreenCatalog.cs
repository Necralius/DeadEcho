using System;
using System.Collections.Generic;
using Project.Core.Services;
using Project.UI.MainMenu.Controllers;

namespace Project.UI.Navigation
{
    public sealed class UiScreenCatalog
    {
        private readonly Dictionary<UiScreenId, Func<IUiScreenController>> _factories = new();

        public UiScreenCatalog(
            IModalService modalService,
            ISaveGameQuery saveGameQuery,
            INewGameService newGameService,
            IApplicationQuitService quitService,
            IAudioSettingsService audioSettings,
            IGraphicsSettingsService graphicsSettings,
            IGraphicsRevertService graphicsRevertService,
            IControlsSettingsService controlsSettings,
            ILoadGameService loadGameService,
            IDeleteSaveService deleteSaveService,
            ICreditsContentProvider creditsContentProvider)
        {
            Register(UiScreenId.MainMenu, () => new MainMenuScreenController(modalService, saveGameQuery, newGameService, quitService));
            Register(UiScreenId.NewGame, () => new TemporaryMenuScreenController(UiScreenId.NewGame, "New Game"));
            Register(UiScreenId.LoadGame, () => new LoadGameScreenController(saveGameQuery, loadGameService, deleteSaveService, modalService));
            Register(UiScreenId.Settings, () => new SettingsScreenController(audioSettings, graphicsSettings, graphicsRevertService, controlsSettings));
            Register(UiScreenId.Audio, () => new TemporaryMenuScreenController(UiScreenId.Audio, "Audio"));
            Register(UiScreenId.Graphics, () => new TemporaryMenuScreenController(UiScreenId.Graphics, "Graphics"));
            Register(UiScreenId.Controls, () => new TemporaryMenuScreenController(UiScreenId.Controls, "Controls"));
            Register(UiScreenId.Credits, () => new CreditsScreenController(creditsContentProvider));
            Register(UiScreenId.Extras, () => new ExtrasScreenController());
        }

        public void Register(UiScreenId screenId, Func<IUiScreenController> factory)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _factories[screenId] = factory;
        }

        public IUiScreenController Create(UiScreenId screenId)
        {
            if (!_factories.TryGetValue(screenId, out Func<IUiScreenController> factory))
                throw new InvalidOperationException($"UI screen '{screenId}' is not registered.");

            return factory.Invoke();
        }
    }
}
