using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Project.Core.Services;
using Project.UI.MainMenu.Controllers;
using Project.UI.Modals;
using Project.UI.Navigation;
using Project.UI.Runtime;
using Project.UI.Services;
using UnityEngine.TestTools;

namespace Project.Tests.PlayMode.UI
{
    public sealed class MainMenuPlayModeTests
    {
        [UnityTest]
        public IEnumerator OpensMenuAndNavigatesToSettings()
        {
            var root = new HeadlessUiRoot();
            var modalService = new ModalService(root);
            var saveQuery = new EmptySaveGameQuery();
            var newGame = new RecordingNewGameService();
            var quit = new RecordingQuitService();
            var audio = new RecordingAudioSettingsService();
            var graphics = new RecordingGraphicsSettingsService();
            var graphicsRevert = new ImmediateGraphicsRevertService();
            var controls = new RecordingControlsSettingsService();
            var load = new RecordingLoadGameService();
            var delete = new RecordingDeleteSaveService();
            var credits = new RecordingCreditsProvider();
            var catalog = new UiScreenCatalog(
                () => new MainMenuScreenController(modalService, saveQuery, newGame, quit),
                () => new LoadGameScreenController(saveQuery, load, delete, modalService),
                () => new SettingsScreenController(audio, graphics, graphicsRevert, controls),
                () => new CreditsScreenController(credits),
                () => new ExtrasScreenController(),
                (screenId, title) => new TemporaryMenuScreenController(screenId, title));
            var uiService = new UiService(root, catalog, new UiNavigationStack(), modalService);

            uiService.Replace(UiScreenId.MainMenu);
            yield return null;

            uiService.Show(UiScreenId.Settings);
            yield return null;

            Assert.AreEqual(UiScreenId.Settings, uiService.CurrentScreenId);
            Assert.IsTrue(uiService.CanGoBack);

            uiService.Dispose();
        }

        private sealed class EmptySaveGameQuery : ISaveGameQuery
        {
            public bool HasAnySave()
            {
                return false;
            }

            public IReadOnlyList<SaveGameSummary> GetAvailableSaves()
            {
                return System.Array.Empty<SaveGameSummary>();
            }
        }

        private sealed class RecordingNewGameService : INewGameService
        {
            public Task StartNewGameAsync() => Task.CompletedTask;
        }

        private sealed class RecordingQuitService : IApplicationQuitService
        {
            public bool IsQuitAvailable => true;
            public void Quit() { }
        }

        private sealed class RecordingAudioSettingsService : IAudioSettingsService
        {
            public float MasterVolume { get; set; } = 1f;
            public float MusicVolume { get; set; } = 1f;
            public float SfxVolume { get; set; } = 1f;
            public bool MuteAll { get; set; }
            public void RestoreDefaults() { }
        }

        private sealed class RecordingGraphicsSettingsService : IGraphicsSettingsService
        {
            public GraphicsSettingsSnapshot Current { get; private set; } = new(DisplayModeOption.Windowed, new ResolutionOption(1280, 720, 60), 0, true, 60);
            public IReadOnlyList<ResolutionOption> AvailableResolutions { get; } = new[] { new ResolutionOption(1280, 720, 60) };
            public IReadOnlyList<string> QualityPresets { get; } = new[] { "Low", "High" };
            public void Apply(GraphicsSettingsSnapshot settings) => Current = settings;
            public void RestoreDefaults() { }
        }

        private sealed class ImmediateGraphicsRevertService : IGraphicsRevertService
        {
            public Task<bool> ApplyWithConfirmationAsync(GraphicsSettingsSnapshot settings, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(true);
        }

        private sealed class RecordingControlsSettingsService : IControlsSettingsService
        {
            public IReadOnlyList<ControlBindingView> GetBindings() => System.Array.Empty<ControlBindingView>();
            public Task<bool> RebindAsync(string actionId, System.Threading.CancellationToken cancellationToken = default) => Task.FromResult(false);
            public bool HasDuplicateBindings() => false;
            public string SaveOverridesAsJson() => "{}";
            public void LoadOverridesFromJson(string json) { }
            public void RestoreDefaults() { }
        }

        private sealed class RecordingLoadGameService : ILoadGameService
        {
            public Task LoadAsync(string saveId) => Task.CompletedTask;
        }

        private sealed class RecordingDeleteSaveService : IDeleteSaveService
        {
            public Task DeleteAsync(string saveId) => Task.CompletedTask;
        }

        private sealed class RecordingCreditsProvider : ICreditsContentProvider
        {
            public IReadOnlyList<CreditsSection> GetSections() => System.Array.Empty<CreditsSection>();
        }
    }
}
