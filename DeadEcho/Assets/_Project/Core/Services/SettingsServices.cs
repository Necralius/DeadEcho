using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Project.Core.Services
{
    public interface IAudioSettingsService
    {
        float MasterVolume { get; set; }
        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
        bool MuteAll { get; set; }
        void RestoreDefaults();
    }

    public enum DisplayModeOption
    {
        Fullscreen,
        Borderless,
        Windowed
    }

    public readonly struct ResolutionOption : IEquatable<ResolutionOption>
    {
        public ResolutionOption(int width, int height, int refreshRate)
        {
            Width = width;
            Height = height;
            RefreshRate = refreshRate;
        }

        public int Width { get; }
        public int Height { get; }
        public int RefreshRate { get; }
        public string Label => RefreshRate > 0 ? $"{Width} x {Height} @ {RefreshRate}Hz" : $"{Width} x {Height}";
        public bool Equals(ResolutionOption other) => Width == other.Width && Height == other.Height && RefreshRate == other.RefreshRate;
        public override bool Equals(object obj) => obj is ResolutionOption other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Width, Height, RefreshRate);
    }

    public readonly struct GraphicsSettingsSnapshot : IEquatable<GraphicsSettingsSnapshot>
    {
        public GraphicsSettingsSnapshot(
            DisplayModeOption displayMode,
            ResolutionOption resolution,
            int qualityPreset,
            bool vSync,
            int fpsLimit)
        {
            DisplayMode = displayMode;
            Resolution = resolution;
            QualityPreset = qualityPreset;
            VSync = vSync;
            FpsLimit = fpsLimit;
        }

        public DisplayModeOption DisplayMode { get; }
        public ResolutionOption Resolution { get; }
        public int QualityPreset { get; }
        public bool VSync { get; }
        public int FpsLimit { get; }
        public bool Equals(GraphicsSettingsSnapshot other) =>
            DisplayMode == other.DisplayMode &&
            Resolution.Equals(other.Resolution) &&
            QualityPreset == other.QualityPreset &&
            VSync == other.VSync &&
            FpsLimit == other.FpsLimit;
        public override bool Equals(object obj) => obj is GraphicsSettingsSnapshot other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(DisplayMode, Resolution, QualityPreset, VSync, FpsLimit);
    }

    public interface IGraphicsSettingsService
    {
        GraphicsSettingsSnapshot Current { get; }
        IReadOnlyList<ResolutionOption> AvailableResolutions { get; }
        IReadOnlyList<string> QualityPresets { get; }
        void Apply(GraphicsSettingsSnapshot settings);
        void RestoreDefaults();
    }

    public interface IGraphicsRevertService
    {
        Task<bool> ApplyWithConfirmationAsync(GraphicsSettingsSnapshot settings, CancellationToken cancellationToken = default);
    }

    public interface IControlsSettingsService
    {
        IReadOnlyList<ControlBindingView> GetBindings();
        Task<bool> RebindAsync(string actionId, CancellationToken cancellationToken = default);
        bool HasDuplicateBindings();
        string SaveOverridesAsJson();
        void LoadOverridesFromJson(string json);
        void RestoreDefaults();
    }

    public readonly struct ControlBindingView
    {
        public ControlBindingView(string actionId, string group, string actionName, string bindingName, string displayPath)
        {
            ActionId = actionId ?? string.Empty;
            Group = group ?? string.Empty;
            ActionName = actionName ?? string.Empty;
            BindingName = bindingName ?? string.Empty;
            DisplayPath = displayPath ?? string.Empty;
        }

        public string ActionId { get; }
        public string Group { get; }
        public string ActionName { get; }
        public string BindingName { get; }
        public string DisplayPath { get; }
    }

    [Serializable]
    public sealed class GameSettingsData
    {
        public int Version = 1;
        public float MasterVolume = 1f;
        public float MusicVolume = 1f;
        public float SfxVolume = 1f;
        public bool MuteAll;
        public int DisplayMode = (int)DisplayModeOption.Borderless;
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;
        public int RefreshRate = 60;
        public int QualityPreset = 0;
        public bool VSync = true;
        public int FpsLimit = 60;
        public string BindingOverridesJson = string.Empty;
    }

    public interface ISettingsStorage
    {
        bool TryLoad(out GameSettingsData data);
        void Save(GameSettingsData data);
        void Reset();
    }
}
