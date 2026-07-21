using System;
using UnityEngine.UIElements;

namespace Project.UI.Runtime
{
    public sealed class HeadlessUiRoot : IUiRoot
    {
        public HeadlessUiRoot()
        {
            Root = new VisualElement { name = "ui-root" };
            ScreenLayer = new VisualElement { name = UiRoot.ScreenLayerName };
            InteractionBlocker = new VisualElement { name = UiRoot.InteractionBlockerName };
            ModalLayer = new VisualElement { name = UiRoot.ModalLayerName };
            LoadingLayer = new VisualElement { name = UiRoot.LoadingLayerName };

            InteractionBlocker.style.display = DisplayStyle.None;
            ModalLayer.style.display = DisplayStyle.None;
            LoadingLayer.style.display = DisplayStyle.None;

            Root.Add(ScreenLayer);
            Root.Add(InteractionBlocker);
            Root.Add(ModalLayer);
            Root.Add(LoadingLayer);
        }

        public VisualElement Root { get; }
        public VisualElement ScreenLayer { get; }
        public VisualElement ModalLayer { get; }
        public VisualElement InteractionBlocker { get; }
        public VisualElement LoadingLayer { get; }
        public VisualElement FocusedElement { get; private set; }

        public event Action CancelRequested;

        public void Focus(VisualElement element)
        {
            FocusedElement = element ?? Root;
            FocusedElement.Focus();
        }

        public void RequestCancel()
        {
            CancelRequested?.Invoke();
        }
    }
}
