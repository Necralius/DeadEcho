using System;
using Project.Core.Services;
using UnityEngine.UIElements;

namespace Project.UI.Navigation
{
    public interface IUiScreenController : IDisposable
    {
        UiScreenId ScreenId { get; }
        VisualElement Root { get; }
        VisualElement DefaultFocus { get; }

        void Open();
        void Close();
    }
}
