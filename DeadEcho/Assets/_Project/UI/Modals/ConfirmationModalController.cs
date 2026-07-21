using System;
using Project.Core.Services;
using UnityEngine;
using UnityEngine.UIElements;

namespace Project.UI.Modals
{
    public sealed class ConfirmationModalController : IDisposable
    {
        private const string ConfirmationTemplatePath = "UI/MainMenu/ConfirmationModal";
        private const string ThemePath = "UI/Shared/MainMenuTheme";

        private readonly Action<bool> _complete;
        private readonly bool _allowCancel;
        private bool _isCompleted;
        private readonly Button _cancelButton;
        private readonly Button _confirmButton;

        public ConfirmationModalController(ConfirmationModalRequest request, Action<bool> complete)
        {
            _complete = complete ?? throw new ArgumentNullException(nameof(complete));
            _allowCancel = request.AllowCancel;
            Root = BuildRoot(request);
            _cancelButton = Require<Button>("cancel-button");
            _confirmButton = Require<Button>("confirm-button");
            _cancelButton.clicked += Cancel;
            _confirmButton.clicked += Confirm;
            _cancelButton.style.display = request.AllowCancel ? DisplayStyle.Flex : DisplayStyle.None;
            DefaultFocus = request.AllowCancel ? _cancelButton : _confirmButton;
        }

        public VisualElement Root { get; }
        public VisualElement DefaultFocus { get; private set; }

        public void Confirm()
        {
            Complete(true);
        }

        public void Cancel()
        {
            if (!_allowCancel)
                return;

            Complete(false);
        }

        public void Dispose()
        {
            _cancelButton.clicked -= Cancel;
            _confirmButton.clicked -= Confirm;
            Root.RemoveFromHierarchy();
        }

        private VisualElement BuildRoot(ConfirmationModalRequest request)
        {
            VisualElement root = LoadTemplate() ?? BuildFallbackRoot();
            ApplyTheme(root);
            RootLabel(root, "confirmation-title").text = request.Title;
            RootLabel(root, "confirmation-message").text = request.Message;
            RootButton(root, "cancel-button").text = request.CancelLabel;
            RootButton(root, "confirm-button").text = request.ConfirmLabel;
            root.EnableInClassList("modal-warning", request.Variant == ModalVariant.Warning);
            root.EnableInClassList("modal-danger", request.Variant == ModalVariant.Danger);
            root.pickingMode = PickingMode.Position;
            return root;
        }

        private static VisualElement LoadTemplate()
        {
            VisualTreeAsset template = Resources.Load<VisualTreeAsset>(ConfirmationTemplatePath);
            TemplateContainer container = template?.CloneTree();
            VisualElement modal = container?.Q<VisualElement>("confirmation-modal");
            modal?.RemoveFromHierarchy();
            return modal;
        }

        private static VisualElement BuildFallbackRoot()
        {
            var overlay = new VisualElement { name = "confirmation-modal" };
            overlay.AddToClassList("modal-overlay");
            overlay.pickingMode = PickingMode.Position;

            var dialog = new VisualElement { name = "confirmation-dialog" };
            dialog.AddToClassList("modal-dialog");
            overlay.Add(dialog);

            var title = new Label { name = "confirmation-title" };
            title.AddToClassList("modal-title");
            dialog.Add(title);

            var message = new Label { name = "confirmation-message" };
            message.AddToClassList("modal-message");
            dialog.Add(message);

            var actions = new VisualElement { name = "confirmation-actions" };
            actions.AddToClassList("modal-actions");
            dialog.Add(actions);

            var cancelButton = new Button { name = "cancel-button" };
            cancelButton.AddToClassList("button-secondary");
            cancelButton.AddToClassList("modal-button");
            var confirmButton = new Button { name = "confirm-button" };
            confirmButton.AddToClassList("button-danger");
            confirmButton.AddToClassList("modal-button");
            actions.Add(cancelButton);
            actions.Add(confirmButton);
            return overlay;
        }

        private static void ApplyTheme(VisualElement root)
        {
            StyleSheet theme = Resources.Load<StyleSheet>(ThemePath);
            if (theme != null)
                root.styleSheets.Add(theme);
        }

        private T Require<T>(string name) where T : VisualElement
        {
            T element = Root.Q<T>(name);
            if (element == null)
                throw new InvalidOperationException($"Confirmation modal UXML is missing '{name}'.");

            return element;
        }

        private static Label RootLabel(VisualElement root, string name)
        {
            Label label = root.Q<Label>(name);
            if (label == null)
                throw new InvalidOperationException($"Confirmation modal UXML is missing '{name}'.");

            return label;
        }

        private static Button RootButton(VisualElement root, string name)
        {
            Button button = root.Q<Button>(name);
            if (button == null)
                throw new InvalidOperationException($"Confirmation modal UXML is missing '{name}'.");

            return button;
        }

        private void Complete(bool result)
        {
            if (_isCompleted)
                return;

            _isCompleted = true;
            _complete.Invoke(result);
        }
    }
}
