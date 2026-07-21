using System;
using UnityEngine.UIElements;

namespace Project.UI.Runtime
{
    public interface IUiRoot
    {
        VisualElement Root { get; }
        VisualElement ScreenLayer { get; }
        VisualElement ModalLayer { get; }
        VisualElement InteractionBlocker { get; }
        event Action CancelRequested;

        void Focus(VisualElement element);
    }
}
