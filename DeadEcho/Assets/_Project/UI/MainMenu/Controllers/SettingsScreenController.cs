using System;
using System.Collections.Generic;
using System.Linq;
using Project.Core.Services;
using Project.UI.Navigation;
using UnityEngine.UIElements;

namespace Project.UI.MainMenu.Controllers
{
    public sealed class SettingsScreenController : IUiScreenController, IUiNavigationRequestSource
    {
        private readonly IAudioSettingsService _audioSettings;
        private readonly IGraphicsSettingsService _graphicsSettings;
        private readonly IGraphicsRevertService _graphicsRevertService;
        private readonly IControlsSettingsService _controlsSettings;
        private readonly VisualElement _content;
        private readonly Button _audioTab;
        private readonly Button _graphicsTab;
        private readonly Button _controlsTab;
        private readonly Button _backButton;
        private SettingsTab _activeTab = SettingsTab.Audio;
        private GraphicsSettingsSnapshot _pendingGraphics;
        private bool _graphicsDirty;

        public SettingsScreenController(
            IAudioSettingsService audioSettings,
            IGraphicsSettingsService graphicsSettings,
            IGraphicsRevertService graphicsRevertService,
            IControlsSettingsService controlsSettings)
        {
            _audioSettings = audioSettings;
            _graphicsSettings = graphicsSettings;
            _graphicsRevertService = graphicsRevertService;
            _controlsSettings = controlsSettings;
            _pendingGraphics = graphicsSettings.Current;

            Root = new VisualElement { name = "settings-screen" };
            Root.AddToClassList("ui-screen");
            Root.AddToClassList("settings-screen");

            var header = new Label("Settings");
            header.AddToClassList("screen-title");
            Root.Add(header);

            var layout = new VisualElement { name = "settings-layout" };
            layout.AddToClassList("settings-layout");
            Root.Add(layout);

            var tabs = new VisualElement { name = "settings-tabs" };
            tabs.AddToClassList("settings-tabs");
            layout.Add(tabs);

            _audioTab = CreateTab("Audio");
            _graphicsTab = CreateTab("Graphics");
            _controlsTab = CreateTab("Controls");
            tabs.Add(_audioTab);
            tabs.Add(_graphicsTab);
            tabs.Add(_controlsTab);

            _content = new VisualElement { name = "settings-content" };
            _content.AddToClassList("settings-content");
            layout.Add(_content);

            var footer = new VisualElement();
            footer.AddToClassList("screen-actions");
            _backButton = CreateButton("Back", "button-secondary");
            footer.Add(_backButton);
            Root.Add(footer);

            DefaultFocus = _audioTab;
            _audioTab.clicked += () => ShowTab(SettingsTab.Audio);
            _graphicsTab.clicked += () => ShowTab(SettingsTab.Graphics);
            _controlsTab.clicked += () => ShowTab(SettingsTab.Controls);
            _backButton.clicked += () => BackRequested?.Invoke();
        }

        public event Action<UiScreenId> ShowRequested { add { } remove { } }
        public event Action<UiScreenId> ReplaceRequested { add { } remove { } }
        public event Action BackRequested;
        public UiScreenId ScreenId => UiScreenId.Settings;
        public VisualElement Root { get; }
        public VisualElement DefaultFocus { get; private set; }

        public void Open()
        {
            Root.style.display = DisplayStyle.Flex;
            ShowTab(_activeTab);
        }

        public void Close()
        {
            Root.RemoveFromHierarchy();
        }

        public void Dispose()
        {
            Close();
        }

        private void ShowTab(SettingsTab tab)
        {
            _activeTab = tab;
            _audioTab.EnableInClassList("ui-tab-selected", tab == SettingsTab.Audio);
            _graphicsTab.EnableInClassList("ui-tab-selected", tab == SettingsTab.Graphics);
            _controlsTab.EnableInClassList("ui-tab-selected", tab == SettingsTab.Controls);
            _content.Clear();

            if (tab == SettingsTab.Audio)
                BuildAudio();
            else if (tab == SettingsTab.Graphics)
                BuildGraphics();
            else
                BuildControls();
        }

        private void BuildAudio()
        {
            AddSlider("Master Volume", _audioSettings.MasterVolume, value => _audioSettings.MasterVolume = value);
            AddSlider("Music Volume", _audioSettings.MusicVolume, value => _audioSettings.MusicVolume = value);
            AddSlider("SFX Volume", _audioSettings.SfxVolume, value => _audioSettings.SfxVolume = value);

            var mute = new Toggle("Mute All") { value = _audioSettings.MuteAll };
            mute.AddToClassList("ui-toggle");
            mute.RegisterValueChangedCallback(evt => _audioSettings.MuteAll = evt.newValue);
            _content.Add(mute);

            Button defaults = CreateButton("Restore Defaults", "button-secondary");
            defaults.clicked += () =>
            {
                _audioSettings.RestoreDefaults();
                ShowTab(SettingsTab.Audio);
            };
            _content.Add(defaults);
        }

        private void BuildGraphics()
        {
            var dirtyLabel = new Label(_graphicsDirty ? "Unapplied graphics changes" : "Graphics changes require Apply.");
            dirtyLabel.AddToClassList(_graphicsDirty ? "settings-dirty" : "screen-placeholder");
            _content.Add(dirtyLabel);

            AddEnumField("Display Mode", _pendingGraphics.DisplayMode, value =>
            {
                _pendingGraphics = new GraphicsSettingsSnapshot(value, _pendingGraphics.Resolution, _pendingGraphics.QualityPreset, _pendingGraphics.VSync, _pendingGraphics.FpsLimit);
                _graphicsDirty = true;
                ShowTab(SettingsTab.Graphics);
            });

            AddPopup("Resolution", _graphicsSettings.AvailableResolutions.Select(option => option.Label).ToList(), _pendingGraphics.Resolution.Label, index =>
            {
                ResolutionOption option = _graphicsSettings.AvailableResolutions[index];
                _pendingGraphics = new GraphicsSettingsSnapshot(_pendingGraphics.DisplayMode, option, _pendingGraphics.QualityPreset, _pendingGraphics.VSync, _pendingGraphics.FpsLimit);
                _graphicsDirty = true;
                ShowTab(SettingsTab.Graphics);
            });

            if (_graphicsSettings.QualityPresets.Count > 0)
            {
                AddPopup("Quality Preset", _graphicsSettings.QualityPresets.ToList(), _graphicsSettings.QualityPresets[Math.Max(0, Math.Min(_pendingGraphics.QualityPreset, _graphicsSettings.QualityPresets.Count - 1))], index =>
                {
                    _pendingGraphics = new GraphicsSettingsSnapshot(_pendingGraphics.DisplayMode, _pendingGraphics.Resolution, index, _pendingGraphics.VSync, _pendingGraphics.FpsLimit);
                    _graphicsDirty = true;
                    ShowTab(SettingsTab.Graphics);
                });
            }

            var vSync = new Toggle("VSync") { value = _pendingGraphics.VSync };
            vSync.AddToClassList("ui-toggle");
            vSync.RegisterValueChangedCallback(evt =>
            {
                _pendingGraphics = new GraphicsSettingsSnapshot(_pendingGraphics.DisplayMode, _pendingGraphics.Resolution, _pendingGraphics.QualityPreset, evt.newValue, _pendingGraphics.FpsLimit);
                _graphicsDirty = true;
            });
            _content.Add(vSync);

            AddIntegerField("FPS Limit", _pendingGraphics.FpsLimit, value =>
            {
                _pendingGraphics = new GraphicsSettingsSnapshot(_pendingGraphics.DisplayMode, _pendingGraphics.Resolution, _pendingGraphics.QualityPreset, _pendingGraphics.VSync, Math.Max(30, value));
                _graphicsDirty = true;
            });

            var actions = new VisualElement();
            actions.AddToClassList("screen-actions");
            Button apply = CreateButton("Apply", "button-primary");
            Button defaults = CreateButton("Restore Defaults", "button-secondary");
            apply.clicked += async () =>
            {
                bool kept = await _graphicsRevertService.ApplyWithConfirmationAsync(_pendingGraphics);
                _pendingGraphics = _graphicsSettings.Current;
                _graphicsDirty = !kept;
                ShowTab(SettingsTab.Graphics);
            };
            defaults.clicked += () =>
            {
                _graphicsSettings.RestoreDefaults();
                _pendingGraphics = _graphicsSettings.Current;
                _graphicsDirty = false;
                ShowTab(SettingsTab.Graphics);
            };
            actions.Add(apply);
            actions.Add(defaults);
            _content.Add(actions);
        }

        private void BuildControls()
        {
            if (_controlsSettings.HasDuplicateBindings())
            {
                var duplicate = new Label("Duplicate bindings detected.");
                duplicate.AddToClassList("settings-dirty");
                _content.Add(duplicate);
            }

            foreach (ControlBindingView binding in _controlsSettings.GetBindings())
            {
                var row = new VisualElement();
                row.AddToClassList("binding-row");
                row.Add(new Label($"{binding.Group} / {binding.ActionName}") { name = "binding-action" });
                row.Add(new Label(binding.DisplayPath) { name = "binding-path" });
                Button rebind = CreateButton("Rebind", "button-secondary");
                rebind.clicked += async () =>
                {
                    rebind.text = "Press a key or button";
                    rebind.SetEnabled(false);
                    await _controlsSettings.RebindAsync(binding.ActionId);
                    ShowTab(SettingsTab.Controls);
                };
                row.Add(rebind);
                _content.Add(row);
            }

            Button defaults = CreateButton("Restore Defaults", "button-secondary");
            defaults.clicked += () =>
            {
                _controlsSettings.RestoreDefaults();
                ShowTab(SettingsTab.Controls);
            };
            _content.Add(defaults);
        }

        private void AddSlider(string label, float value, Action<float> changed)
        {
            var row = new VisualElement();
            row.AddToClassList("setting-row");
            var title = new Label(label);
            var valueLabel = new Label($"{Math.Round(value * 100f)}%");
            var slider = new Slider(0f, 1f) { value = value };
            slider.AddToClassList("ui-slider");
            slider.RegisterValueChangedCallback(evt =>
            {
                valueLabel.text = $"{Math.Round(evt.newValue * 100f)}%";
                changed(evt.newValue);
            });
            row.Add(title);
            row.Add(slider);
            row.Add(valueLabel);
            _content.Add(row);
        }

        private void AddEnumField(string label, DisplayModeOption value, Action<DisplayModeOption> changed)
        {
            AddPopup(label, Enum.GetNames(typeof(DisplayModeOption)).ToList(), value.ToString(), index => changed((DisplayModeOption)index));
        }

        private void AddPopup(string label, List<string> choices, string value, Action<int> changed)
        {
            if (choices.Count == 0)
                return;

            var field = new PopupField<string>(label, choices, Math.Max(0, choices.IndexOf(value)));
            field.AddToClassList("ui-dropdown");
            field.RegisterValueChangedCallback(evt => changed(choices.IndexOf(evt.newValue)));
            _content.Add(field);
        }

        private void AddIntegerField(string label, int value, Action<int> changed)
        {
            var field = new IntegerField(label) { value = value };
            field.RegisterValueChangedCallback(evt => changed(evt.newValue));
            _content.Add(field);
        }

        private static Button CreateTab(string text)
        {
            Button button = CreateButton(text, "button-secondary");
            button.AddToClassList("settings-tab");
            return button;
        }

        private static Button CreateButton(string text, string className)
        {
            var button = new Button { text = text };
            button.AddToClassList(className);
            button.AddToClassList("menu-button");
            return button;
        }

        private enum SettingsTab
        {
            Audio,
            Graphics,
            Controls
        }
    }
}
