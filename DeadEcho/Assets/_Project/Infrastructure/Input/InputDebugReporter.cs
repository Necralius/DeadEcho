using Project.Core.Services;
using UnityEngine;
using VContainer;

namespace Project.Infrastructure.Input
{
    public sealed class InputDebugReporter : MonoBehaviour
    {
        [SerializeField] private bool logToConsole;
        [SerializeField] private bool drawOnScreen = true;
        [SerializeField] private float logIntervalSeconds = 1f;

        private IInputService _inputService;
        private float _nextLogTime;

        [Inject]
        public void Construct(IInputService inputService)
        {
            _inputService = inputService;
        }

        private void Update()
        {
            if (!logToConsole || _inputService == null || Time.unscaledTime < _nextLogTime)
                return;

            _nextLogTime = Time.unscaledTime + logIntervalSeconds;
            Debug.Log($"[InputDebugReporter] Move={_inputService.Move} Look={_inputService.Look} Run={_inputService.IsRunning} Crouch={_inputService.IsCrouching}");
        }

        private void OnGUI()
        {
            if (!drawOnScreen || _inputService == null)
                return;

            GUILayout.Label($"Move: {_inputService.Move}");
            GUILayout.Label($"Look: {_inputService.Look}");
            GUILayout.Label($"Run: {_inputService.IsRunning}");
            GUILayout.Label($"Crouch: {_inputService.IsCrouching}");
            GUILayout.Label($"Gameplay Input: {_inputService.IsGameplayInputEnabled}");
            GUILayout.Label($"UI Input: {_inputService.IsUiInputEnabled}");
        }
    }
}
