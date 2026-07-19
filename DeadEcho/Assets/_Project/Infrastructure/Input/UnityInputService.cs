using System;
using Project.Core.Services;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Project.Infrastructure.Input
{
    public sealed class UnityInputService : IInputService, IDisposable
    {
        private readonly InputActionAsset _inputActions;
        private readonly InputActionMap _gameplayMap;
        private readonly InputActionMap _uiMap;
        private readonly InputAction _moveAction;
        private readonly InputAction _lookAction;
        private readonly InputAction _runAction;
        private readonly InputAction _crouchAction;
        private readonly InputAction _interactAction;
        private readonly InputAction _useAction;
        private readonly InputAction _pauseAction;
        private readonly InputAction _uiPauseAction;

        public UnityInputService(InputActionAsset inputActions)
        {
            if (inputActions == null)
                throw new ArgumentNullException(nameof(inputActions));

            _inputActions = UnityEngine.Object.Instantiate(inputActions);
            _gameplayMap = RequireActionMap("Gameplay");
            _uiMap = _inputActions.FindActionMap("UI", throwIfNotFound: false);

            _moveAction = RequireGameplayAction("Move");
            _lookAction = RequireGameplayAction("Look");
            _runAction = RequireGameplayAction("Run");
            _crouchAction = RequireGameplayAction("Crouch");
            _interactAction = RequireGameplayAction("Interact");
            _useAction = RequireGameplayAction("Use");
            _pauseAction = RequireGameplayAction("Pause");
            _uiPauseAction = _uiMap?.FindAction("Pause", throwIfNotFound: false);

            _interactAction.performed += OnInteractPerformed;
            _useAction.performed += OnUsePerformed;
            _pauseAction.performed += OnPausePerformed;
            if (_uiPauseAction != null)
                _uiPauseAction.performed += OnPausePerformed;
        }

        public Vector2 Move => IsGameplayInputEnabled ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
        public Vector2 Look => IsGameplayInputEnabled ? _lookAction.ReadValue<Vector2>() : Vector2.zero;
        public bool IsRunning => IsGameplayInputEnabled && _runAction.IsPressed();
        public bool IsCrouching => IsGameplayInputEnabled && _crouchAction.IsPressed();
        public bool IsGameplayInputEnabled => _gameplayMap.enabled;
        public bool IsUiInputEnabled => _uiMap != null && _uiMap.enabled;

        public event Action InteractPressed;
        public event Action UsePressed;
        public event Action PausePressed;

        public void EnableGameplayInput() => _gameplayMap.Enable();
        public void DisableGameplayInput() => _gameplayMap.Disable();
        public void EnableUiInput() => _uiMap?.Enable();
        public void DisableUiInput() => _uiMap?.Disable();

        public void Dispose()
        {
            _interactAction.performed -= OnInteractPerformed;
            _useAction.performed -= OnUsePerformed;
            _pauseAction.performed -= OnPausePerformed;
            if (_uiPauseAction != null)
                _uiPauseAction.performed -= OnPausePerformed;
            UnityEngine.Object.Destroy(_inputActions);
        }

        private InputActionMap RequireActionMap(string mapName)
        {
            return _inputActions.FindActionMap(mapName, throwIfNotFound: true);
        }

        private InputAction RequireGameplayAction(string actionName)
        {
            return _gameplayMap.FindAction(actionName, throwIfNotFound: true);
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (IsGameplayInputEnabled)
                InteractPressed?.Invoke();
        }

        private void OnUsePerformed(InputAction.CallbackContext context)
        {
            if (IsGameplayInputEnabled)
                UsePressed?.Invoke();
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            if (IsGameplayInputEnabled || IsUiInputEnabled)
                PausePressed?.Invoke();
        }
    }
}
