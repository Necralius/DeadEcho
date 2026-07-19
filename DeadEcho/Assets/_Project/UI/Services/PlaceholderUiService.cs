using Project.Core.Services;
using UnityEngine;

namespace Project.UI.Services
{
    public sealed class PlaceholderUiService : IUiService
    {
        public void ShowScreen(string screenId)
        {
            Debug.Log($"[PlaceholderUiService] Show screen requested for '{screenId}'.");
        }

        public void HideScreen(string screenId)
        {
            Debug.Log($"[PlaceholderUiService] Hide screen requested for '{screenId}'.");
        }
    }
}
