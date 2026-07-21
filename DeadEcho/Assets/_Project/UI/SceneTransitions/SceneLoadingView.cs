using System;
using System.Threading;
using System.Threading.Tasks;
using Project.Core.Services;
using Project.UI.Runtime;
using UnityEngine.UIElements;

namespace Project.UI.SceneTransitions
{
    public sealed class SceneLoadingView : ISceneLoadingView
    {
        private readonly IUiRoot _uiRoot;
        private readonly VisualElement _root;
        private readonly VisualElement _progressFill;
        private readonly Label _progressLabel;
        private readonly Label _statusLabel;
        private readonly Label _tipLabel;
        private readonly VisualElement _errorPanel;
        private readonly Label _errorTitle;
        private readonly Label _errorMessage;
        private float _progress;

        public SceneLoadingView(IUiRoot uiRoot)
        {
            _uiRoot = uiRoot ?? throw new ArgumentNullException(nameof(uiRoot));
            _root = BuildRoot();
            _progressFill = _root.Q<VisualElement>("loading-progress-fill");
            _progressLabel = _root.Q<Label>("loading-progress-label");
            _statusLabel = _root.Q<Label>("loading-status-label");
            _tipLabel = _root.Q<Label>("loading-tip-label");
            _errorPanel = _root.Q<VisualElement>("loading-error-panel");
            _errorTitle = _root.Q<Label>("loading-error-title");
            _errorMessage = _root.Q<Label>("loading-error-message");
        }

        public Task ShowAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (_root.parent == null)
                _uiRoot.LoadingLayer.Add(_root);

            _uiRoot.LoadingLayer.style.display = DisplayStyle.Flex;
            _uiRoot.LoadingLayer.pickingMode = PickingMode.Position;
            _errorPanel.style.display = DisplayStyle.None;
            _root.style.opacity = 1f;
            SetProgress(0f);
            SetStatus("Preparando...");
            return Task.CompletedTask;
        }

        public void SetProgress(float normalizedProgress)
        {
            _progress = Math.Max(_progress, Math.Max(0f, Math.Min(1f, normalizedProgress)));
            _progressFill.style.width = Length.Percent(_progress * 100f);
            _progressLabel.text = $"{Math.Floor(_progress * 100f):0}%";
        }

        public void SetStatus(string status)
        {
            _statusLabel.text = string.IsNullOrWhiteSpace(status) ? "Carregando..." : status;
        }

        public void SetTip(string tip)
        {
            _tipLabel.text = tip ?? string.Empty;
            _tipLabel.style.display = string.IsNullOrWhiteSpace(tip) ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void ShowError(SceneTransitionError error)
        {
            _errorTitle.text = error?.Title ?? "Loading Failed";
            _errorMessage.text = error?.Message ?? "The requested scene could not be loaded.";
            _errorPanel.style.display = DisplayStyle.Flex;
            _statusLabel.text = "Erro";
        }

        public Task HideAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _root.RemoveFromHierarchy();
            _uiRoot.LoadingLayer.Clear();
            _uiRoot.LoadingLayer.style.display = DisplayStyle.None;
            _progress = 0f;
            return Task.CompletedTask;
        }

        private static VisualElement BuildRoot()
        {
            var root = new VisualElement { name = "scene-loading-root" };
            root.AddToClassList("scene-loading-root");
            root.pickingMode = PickingMode.Position;

            var background = new VisualElement { name = "loading-background" };
            background.AddToClassList("loading-background");
            root.Add(background);

            var overlay = new VisualElement { name = "loading-overlay" };
            overlay.AddToClassList("loading-overlay");
            root.Add(overlay);

            var content = new VisualElement { name = "loading-content" };
            content.AddToClassList("loading-content");
            root.Add(content);

            var title = new Label("Dead Echo") { name = "loading-title" };
            title.AddToClassList("loading-title");
            content.Add(title);

            var status = new Label("Preparando...") { name = "loading-status-label" };
            status.AddToClassList("loading-status-label");
            content.Add(status);

            var track = new VisualElement { name = "loading-progress-track" };
            track.AddToClassList("loading-progress-track");
            var fill = new VisualElement { name = "loading-progress-fill" };
            fill.AddToClassList("loading-progress-fill");
            track.Add(fill);
            content.Add(track);

            var progress = new Label("0%") { name = "loading-progress-label" };
            progress.AddToClassList("loading-progress-label");
            content.Add(progress);

            var tip = new Label { name = "loading-tip-label" };
            tip.AddToClassList("loading-tip-label");
            tip.style.display = DisplayStyle.None;
            content.Add(tip);

            var spinner = new Label("...") { name = "loading-spinner" };
            spinner.AddToClassList("loading-spinner");
            content.Add(spinner);

            var error = new VisualElement { name = "loading-error-panel" };
            error.AddToClassList("loading-error-panel");
            error.style.display = DisplayStyle.None;
            error.Add(new Label("Loading Failed") { name = "loading-error-title" });
            error.Q<Label>("loading-error-title").AddToClassList("loading-error-title");
            error.Add(new Label("The requested scene could not be loaded.") { name = "loading-error-message" });
            error.Q<Label>("loading-error-message").AddToClassList("loading-error-message");
            var actions = new VisualElement { name = "loading-error-actions" };
            actions.AddToClassList("loading-error-actions");
            actions.Add(CreateErrorButton("loading-retry-button", "Retry"));
            actions.Add(CreateErrorButton("loading-main-menu-button", "Main Menu"));
            error.Add(actions);
            content.Add(error);

            return root;
        }

        private static Button CreateErrorButton(string name, string text)
        {
            var button = new Button { name = name, text = text };
            button.AddToClassList("menu-button");
            button.AddToClassList("button-secondary");
            button.SetEnabled(false);
            return button;
        }
    }
}
