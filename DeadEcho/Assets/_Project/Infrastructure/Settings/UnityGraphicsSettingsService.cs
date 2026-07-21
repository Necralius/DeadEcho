using System.Collections.Generic;
using System.Linq;
using Project.Core.Services;
using UnityEngine;

namespace Project.Infrastructure.Settings
{
    public sealed class UnityGraphicsSettingsService : IGraphicsSettingsService
    {
        private readonly ISettingsStorage _storage;
        private readonly List<ResolutionOption> _resolutions;
        private GameSettingsData _data;

        public UnityGraphicsSettingsService(ISettingsStorage storage)
        {
            _storage = storage;
            _data = storage != null && storage.TryLoad(out GameSettingsData loaded) ? loaded : CreateDefaultData();
            _resolutions = DeduplicateResolutions(Screen.resolutions.Select(ToOption)).ToList();
            if (_resolutions.Count == 0)
                _resolutions.Add(new ResolutionOption(Screen.width, Screen.height, GetRefreshRate(Screen.currentResolution)));
        }

        public GraphicsSettingsSnapshot Current => new(
            (DisplayModeOption)_data.DisplayMode,
            new ResolutionOption(_data.ResolutionWidth, _data.ResolutionHeight, _data.RefreshRate),
            _data.QualityPreset,
            _data.VSync,
            _data.FpsLimit);

        public IReadOnlyList<ResolutionOption> AvailableResolutions => _resolutions;
        public IReadOnlyList<string> QualityPresets => QualitySettings.names;

        public void Apply(GraphicsSettingsSnapshot settings)
        {
            _data.DisplayMode = (int)settings.DisplayMode;
            _data.ResolutionWidth = settings.Resolution.Width;
            _data.ResolutionHeight = settings.Resolution.Height;
            _data.RefreshRate = settings.Resolution.RefreshRate;
            _data.QualityPreset = Mathf.Clamp(settings.QualityPreset, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
            _data.VSync = settings.VSync;
            _data.FpsLimit = settings.FpsLimit;

            QualitySettings.SetQualityLevel(_data.QualityPreset, true);
            QualitySettings.vSyncCount = _data.VSync ? 1 : 0;
            Application.targetFrameRate = _data.VSync ? -1 : _data.FpsLimit;
            Screen.SetResolution(_data.ResolutionWidth, _data.ResolutionHeight, ToFullScreenMode(settings.DisplayMode));
            _storage?.Save(_data);
        }

        public void RestoreDefaults()
        {
            _data = CreateDefaultData();
            Apply(Current);
        }

        public static IReadOnlyList<ResolutionOption> DeduplicateResolutions(IEnumerable<ResolutionOption> source)
        {
            return source
                .Where(option => option.Width > 0 && option.Height > 0)
                .GroupBy(option => new { option.Width, option.Height, option.RefreshRate })
                .Select(group => group.First())
                .OrderBy(option => option.Width)
                .ThenBy(option => option.Height)
                .ThenBy(option => option.RefreshRate)
                .ToList();
        }

        private static GameSettingsData CreateDefaultData()
        {
            return new GameSettingsData
            {
                DisplayMode = (int)DisplayModeOption.Borderless,
                ResolutionWidth = Screen.currentResolution.width > 0 ? Screen.currentResolution.width : 1920,
                ResolutionHeight = Screen.currentResolution.height > 0 ? Screen.currentResolution.height : 1080,
                RefreshRate = GetRefreshRate(Screen.currentResolution) > 0 ? GetRefreshRate(Screen.currentResolution) : 60,
                QualityPreset = QualitySettings.GetQualityLevel(),
                VSync = QualitySettings.vSyncCount > 0,
                FpsLimit = Application.targetFrameRate > 0 ? Application.targetFrameRate : 60
            };
        }

        private static ResolutionOption ToOption(Resolution resolution)
        {
            return new ResolutionOption(resolution.width, resolution.height, GetRefreshRate(resolution));
        }

        private static int GetRefreshRate(Resolution resolution)
        {
            return (int)System.Math.Round(resolution.refreshRateRatio.value);
        }

        private static FullScreenMode ToFullScreenMode(DisplayModeOption mode)
        {
            return mode switch
            {
                DisplayModeOption.Fullscreen => FullScreenMode.ExclusiveFullScreen,
                DisplayModeOption.Windowed => FullScreenMode.Windowed,
                _ => FullScreenMode.FullScreenWindow
            };
        }
    }
}
