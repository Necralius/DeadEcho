using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Project.Core.Services;
using Project.Infrastructure.Settings;

namespace Project.Tests.EditMode.UI
{
    public sealed class SettingsServicesTests
    {
        [Test]
        public void LinearToDecibels_ProtectsZero()
        {
            Assert.AreEqual(-80f, UnityAudioSettingsService.LinearToDecibels(0f));
            Assert.AreEqual(0f, UnityAudioSettingsService.LinearToDecibels(1f), 0.001f);
        }

        [Test]
        public void Audio_RestoreDefaults_ResetsVolumes()
        {
            var storage = new MemorySettingsStorage();
            var service = new UnityAudioSettingsService(storage);

            service.MasterVolume = 0.2f;
            service.MusicVolume = 0.3f;
            service.SfxVolume = 0.4f;
            service.MuteAll = true;
            service.RestoreDefaults();

            Assert.AreEqual(1f, service.MasterVolume);
            Assert.AreEqual(1f, service.MusicVolume);
            Assert.AreEqual(1f, service.SfxVolume);
            Assert.IsFalse(service.MuteAll);
        }

        [Test]
        public void SettingsStorage_SavesAndLoads()
        {
            var storage = new MemorySettingsStorage();
            storage.Save(new GameSettingsData { MasterVolume = 0.25f, BindingOverridesJson = "{\"bindings\":[]}" });

            Assert.IsTrue(storage.TryLoad(out GameSettingsData data));
            Assert.AreEqual(0.25f, data.MasterVolume);
            Assert.AreEqual("{\"bindings\":[]}", data.BindingOverridesJson);
        }

        [Test]
        public void DeduplicateResolutions_RemovesDuplicatesAndSorts()
        {
            IReadOnlyList<ResolutionOption> result = UnityGraphicsSettingsService.DeduplicateResolutions(new[]
            {
                new ResolutionOption(1920, 1080, 60),
                new ResolutionOption(1280, 720, 60),
                new ResolutionOption(1920, 1080, 60)
            });

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(new ResolutionOption(1280, 720, 60), result[0]);
            Assert.AreEqual(new ResolutionOption(1920, 1080, 60), result[1]);
        }

        [Test]
        public async Task GraphicsRevert_RevertsWhenNotConfirmed()
        {
            var graphics = new MemoryGraphicsSettingsService();
            var modal = new DecliningModalService();
            var service = new GraphicsRevertService(graphics, modal);
            var changed = new GraphicsSettingsSnapshot(DisplayModeOption.Windowed, new ResolutionOption(1920, 1080, 60), 1, false, 120);

            bool confirmed = await service.ApplyWithConfirmationAsync(changed);

            Assert.IsFalse(confirmed);
            Assert.AreEqual(new ResolutionOption(1280, 720, 60), graphics.Current.Resolution);
        }

        [Test]
        public void BindingOverrides_CanSerialize()
        {
            var controls = new MemoryControlsSettingsService();
            controls.LoadOverridesFromJson("{\"bindings\":[]}");

            Assert.AreEqual("{\"bindings\":[]}", controls.SaveOverridesAsJson());
        }

        [Test]
        public async Task Rebinding_CanCancel()
        {
            var controls = new MemoryControlsSettingsService();
            using var source = new CancellationTokenSource();
            source.Cancel();

            bool result = await controls.RebindAsync("Move", source.Token);

            Assert.IsFalse(result);
        }

        private sealed class MemorySettingsStorage : ISettingsStorage
        {
            private GameSettingsData _data;
            public bool TryLoad(out GameSettingsData data)
            {
                data = _data;
                return data != null;
            }
            public void Save(GameSettingsData data) => _data = data;
            public void Reset() => _data = null;
        }

        private sealed class MemoryGraphicsSettingsService : IGraphicsSettingsService
        {
            public GraphicsSettingsSnapshot Current { get; private set; } = new(DisplayModeOption.Windowed, new ResolutionOption(1280, 720, 60), 0, true, 60);
            public IReadOnlyList<ResolutionOption> AvailableResolutions { get; } = new[] { new ResolutionOption(1280, 720, 60), new ResolutionOption(1920, 1080, 60) };
            public IReadOnlyList<string> QualityPresets { get; } = new[] { "Low", "High" };
            public void Apply(GraphicsSettingsSnapshot settings) => Current = settings;
            public void RestoreDefaults() => Current = new GraphicsSettingsSnapshot(DisplayModeOption.Windowed, new ResolutionOption(1280, 720, 60), 0, true, 60);
        }

        private sealed class DecliningModalService : IModalService
        {
            public Task<bool> ConfirmAsync(ConfirmationModalRequest request) => Task.FromResult(false);
            public Task InformationAsync(string title, string message, string confirmLabel = "OK") => Task.CompletedTask;
            public Task ErrorAsync(string title, string message, string confirmLabel = "OK") => Task.CompletedTask;
        }

        private sealed class MemoryControlsSettingsService : IControlsSettingsService
        {
            private string _json = string.Empty;
            public IReadOnlyList<ControlBindingView> GetBindings() => System.Array.Empty<ControlBindingView>();
            public Task<bool> RebindAsync(string actionId, CancellationToken cancellationToken = default) => Task.FromResult(!cancellationToken.IsCancellationRequested);
            public bool HasDuplicateBindings() => false;
            public string SaveOverridesAsJson() => _json;
            public void LoadOverridesFromJson(string json) => _json = json;
            public void RestoreDefaults() => _json = string.Empty;
        }
    }
}
