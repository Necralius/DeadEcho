using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using Project.Core.Services;
using Project.UI.Modals;
using Project.UI.Navigation;
using Project.UI.Runtime;
using Project.UI.Services;
using UnityEngine.UIElements;

namespace Project.Tests.EditMode.UI
{
    public sealed class UiServiceNavigationTests
    {
        private HeadlessUiRoot _root;
        private ModalService _modalService;
        private UiService _uiService;

        [SetUp]
        public void SetUp()
        {
            _root = new HeadlessUiRoot();
            _modalService = new ModalService(_root);
            _uiService = new UiService(
                _root,
                new UiScreenCatalog(
                    _modalService,
                    new EmptySaveGameQuery(),
                    new RecordingNewGameService(),
                    new RecordingQuitService(),
                    new RecordingAudioSettingsService(),
                    new RecordingGraphicsSettingsService(),
                    new ImmediateGraphicsRevertService(),
                    new RecordingControlsSettingsService(),
                    new RecordingLoadGameService(),
                    new RecordingDeleteSaveService(),
                    new RecordingCreditsProvider()),
                new UiNavigationStack(),
                _modalService);
        }

        [TearDown]
        public void TearDown()
        {
            _uiService.Dispose();
        }

        [Test]
        public void Replace_OpensFirstScreen()
        {
            _uiService.Replace(UiScreenId.MainMenu);

            Assert.AreEqual(UiScreenId.MainMenu, _uiService.CurrentScreenId);
            Assert.AreEqual(1, _root.ScreenLayer.childCount);
            Assert.NotNull(_root.FocusedElement);
        }

        [Test]
        public void Show_PushesCurrentScreenAndOpensNext()
        {
            _uiService.Replace(UiScreenId.MainMenu);
            _uiService.Show(UiScreenId.Settings);

            Assert.AreEqual(UiScreenId.Settings, _uiService.CurrentScreenId);
            Assert.IsTrue(_uiService.CanGoBack);
            Assert.AreEqual(1, _root.ScreenLayer.childCount);
        }

        [Test]
        public void Back_ReturnsToPreviousScreen()
        {
            _uiService.Replace(UiScreenId.MainMenu);
            _uiService.Show(UiScreenId.Settings);

            _uiService.Back();

            Assert.AreEqual(UiScreenId.MainMenu, _uiService.CurrentScreenId);
            Assert.IsFalse(_uiService.CanGoBack);
            Assert.AreEqual(1, _root.ScreenLayer.childCount);
        }

        [Test]
        public void Replace_DoesNotPreservePreviousScreen()
        {
            _uiService.Replace(UiScreenId.MainMenu);
            _uiService.Replace(UiScreenId.Settings);

            Assert.AreEqual(UiScreenId.Settings, _uiService.CurrentScreenId);
            Assert.IsFalse(_uiService.CanGoBack);
        }

        [Test]
        public void Back_DoesNothingWhenStackIsEmpty()
        {
            _uiService.Replace(UiScreenId.MainMenu);

            _uiService.Back();

            Assert.AreEqual(UiScreenId.MainMenu, _uiService.CurrentScreenId);
            Assert.AreEqual(1, _root.ScreenLayer.childCount);
        }

        [Test]
        public void OpenModal_BlocksScreenInteractionAndNavigation()
        {
            _uiService.Replace(UiScreenId.MainMenu);

            _modalService.ConfirmAsync(new ConfirmationModalRequest("Quit", "Quit game?"));
            _uiService.Show(UiScreenId.Settings);

            Assert.AreEqual(UiScreenId.MainMenu, _uiService.CurrentScreenId);
            Assert.AreEqual(DisplayStyle.Flex, _root.InteractionBlocker.style.display.value);
            Assert.AreEqual(1, _root.ModalLayer.childCount);
        }

        [Test]
        public async Task ConfirmAsync_ReturnsPositiveResult()
        {
            var task = _modalService.ConfirmAsync(new ConfirmationModalRequest("New Game", "Start?"));

            Assert.IsTrue(_modalService.TryCompleteActiveModal(true));

            bool result = await task;
            Assert.IsTrue(result);
            Assert.AreEqual(0, _root.ModalLayer.childCount);
        }

        [Test]
        public async Task ConfirmAsync_ReturnsNegativeResult()
        {
            var task = _modalService.ConfirmAsync(new ConfirmationModalRequest("Quit", "Quit game?"));

            Assert.IsTrue(_modalService.TryCancelActiveModal());

            bool result = await task;
            Assert.IsFalse(result);
            Assert.AreEqual(0, _root.ModalLayer.childCount);
        }

        [Test]
        public void ConfirmAsync_RejectsSecondModal()
        {
            _modalService.ConfirmAsync(new ConfirmationModalRequest("First", "Open"));

            Assert.Throws<System.InvalidOperationException>(
                () => _modalService.ConfirmAsync(new ConfirmationModalRequest("Second", "Blocked")));
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
            public void RestoreDefaults()
            {
                MasterVolume = 1f;
                MusicVolume = 1f;
                SfxVolume = 1f;
                MuteAll = false;
            }
        }

        private sealed class RecordingGraphicsSettingsService : IGraphicsSettingsService
        {
            public GraphicsSettingsSnapshot Current { get; private set; } = new(DisplayModeOption.Windowed, new ResolutionOption(1280, 720, 60), 0, true, 60);
            public IReadOnlyList<ResolutionOption> AvailableResolutions { get; } = new[] { new ResolutionOption(1280, 720, 60) };
            public IReadOnlyList<string> QualityPresets { get; } = new[] { "Low", "High" };
            public void Apply(GraphicsSettingsSnapshot settings) => Current = settings;
            public void RestoreDefaults() => Current = new GraphicsSettingsSnapshot(DisplayModeOption.Windowed, new ResolutionOption(1280, 720, 60), 0, true, 60);
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
