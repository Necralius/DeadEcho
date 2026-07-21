using Project.Core.Services;
using UnityEngine;

namespace Project.Infrastructure.UI
{
    public sealed class TemporaryNewGameFlow : INewGameFlow
    {
        public void StartNewGame()
        {
            Debug.Log("[TemporaryNewGameFlow] New game requested.");
        }
    }
}
