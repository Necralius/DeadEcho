using System;
using UnityEngine;

namespace Project.Core.Services
{
    public interface IInputService
    {
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool IsRunning { get; }
        bool IsCrouching { get; }
        bool IsGameplayInputEnabled { get; }
        bool IsUiInputEnabled { get; }

        event Action InteractPressed;
        event Action UsePressed;
        event Action PausePressed;

        void EnableGameplayInput();
        void DisableGameplayInput();
        void EnableUiInput();
        void DisableUiInput();
    }
}
