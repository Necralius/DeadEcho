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
            Func<MainMenuScreenController> mainMenuFactory,
            Func<LoadGameScreenController> loadGameFactory,
            Func<SettingsScreenController> settingsFactory,
            Func<CreditsScreenController> creditsFactory,
            Func<ExtrasScreenController> extrasFactory,
            Func<UiScreenId, string, TemporaryMenuScreenController> temporaryFactory)
        {
            Register(UiScreenId.MainMenu, mainMenuFactory);
            Register(UiScreenId.NewGame, () => temporaryFactory(UiScreenId.NewGame, "New Game"));
            Register(UiScreenId.LoadGame, loadGameFactory);
            Register(UiScreenId.Settings, settingsFactory);
            Register(UiScreenId.Audio, () => temporaryFactory(UiScreenId.Audio, "Audio"));
            Register(UiScreenId.Graphics, () => temporaryFactory(UiScreenId.Graphics, "Graphics"));
            Register(UiScreenId.Controls, () => temporaryFactory(UiScreenId.Controls, "Controls"));
            Register(UiScreenId.Credits, creditsFactory);
            Register(UiScreenId.Extras, extrasFactory);
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
