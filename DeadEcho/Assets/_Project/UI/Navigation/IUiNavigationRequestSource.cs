using System;
using Project.Core.Services;

namespace Project.UI.Navigation
{
    public interface IUiNavigationRequestSource
    {
        event Action<UiScreenId> ShowRequested;
        event Action<UiScreenId> ReplaceRequested;
        event Action BackRequested;
    }
}
