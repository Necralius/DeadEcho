using Project.Core.Services;
using UnityEngine;

namespace Project.UI.Services
{
    public sealed class PlaceholderUiService : IUiService
    {
        public bool CanGoBack => false;

        public void Show(UiScreenId screenId)
        {
            Debug.Log($"[PlaceholderUiService] Show screen requested for '{screenId}'.");
        }

        public void Replace(UiScreenId screenId)
        {
            Debug.Log($"[PlaceholderUiService] Replace screen requested for '{screenId}'.");
        }

        public void Back()
        {
            Debug.Log("[PlaceholderUiService] Back requested.");
        }

        public void CloseCurrent()
        {
            Debug.Log("[PlaceholderUiService] Close current screen requested.");
        }
    }
}
