using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Project.Core.Services;
using UnityEngine.InputSystem;

namespace Project.Infrastructure.Settings
{
    public sealed class UnityControlsSettingsService : IControlsSettingsService, IDisposable
    {
        private readonly InputActionAsset _inputActions;
        private readonly ISettingsStorage _storage;
        private InputActionRebindingExtensions.RebindingOperation _rebindingOperation;

        public UnityControlsSettingsService(InputActionAsset inputActions, ISettingsStorage storage)
        {
            _inputActions = inputActions != null ? UnityEngine.Object.Instantiate(inputActions) : throw new ArgumentNullException(nameof(inputActions));
            _storage = storage;
            if (_storage != null && _storage.TryLoad(out GameSettingsData data) && !string.IsNullOrWhiteSpace(data.BindingOverridesJson))
                LoadOverridesFromJson(data.BindingOverridesJson);
        }

        public IReadOnlyList<ControlBindingView> GetBindings()
        {
            var result = new List<ControlBindingView>();
            foreach (InputActionMap map in _inputActions.actionMaps)
            {
                foreach (InputAction action in map.actions)
                {
                    for (int index = 0; index < action.bindings.Count; index++)
                    {
                        InputBinding binding = action.bindings[index];
                        if (binding.isComposite)
                            continue;

                        result.Add(new ControlBindingView(
                            $"{action.id}:{index}",
                            map.name,
                            action.name,
                            string.IsNullOrWhiteSpace(binding.name) ? binding.groups : binding.name,
                            action.GetBindingDisplayString(index)));
                    }
                }
            }

            return result;
        }

        public Task<bool> RebindAsync(string actionId, CancellationToken cancellationToken = default)
        {
            if (!TryParseActionId(actionId, out InputAction action, out int bindingIndex))
                return Task.FromResult(false);

            _rebindingOperation?.Dispose();
            var completion = new TaskCompletionSource<bool>();
            action.Disable();
            _rebindingOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnCancel(operation => CompleteRebind(action, operation, completion, false))
                .OnComplete(operation => CompleteRebind(action, operation, completion, true));

            cancellationToken.Register(() =>
            {
                if (_rebindingOperation != null)
                    _rebindingOperation.Cancel();
            });

            _rebindingOperation.Start();
            return completion.Task;
        }

        public bool HasDuplicateBindings()
        {
            return _inputActions.actionMaps
                .SelectMany(map => map.actions)
                .SelectMany(action => action.bindings.Where(binding => !binding.isComposite && !binding.isPartOfComposite))
                .Where(binding => !string.IsNullOrWhiteSpace(binding.effectivePath))
                .GroupBy(binding => $"{binding.groups}:{binding.effectivePath}")
                .Any(group => group.Count() > 1);
        }

        public string SaveOverridesAsJson()
        {
            return _inputActions.SaveBindingOverridesAsJson();
        }

        public void LoadOverridesFromJson(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
                _inputActions.LoadBindingOverridesFromJson(json);
        }

        public void RestoreDefaults()
        {
            _inputActions.RemoveAllBindingOverrides();
            SaveOverrides();
        }

        public void Dispose()
        {
            _rebindingOperation?.Dispose();
            UnityEngine.Object.Destroy(_inputActions);
        }

        private bool TryParseActionId(string actionId, out InputAction action, out int bindingIndex)
        {
            action = null;
            bindingIndex = -1;
            string[] parts = (actionId ?? string.Empty).Split(':');
            if (parts.Length != 2 || !Guid.TryParse(parts[0], out Guid actionGuid) || !int.TryParse(parts[1], out bindingIndex))
                return false;

            action = _inputActions.FindAction(actionGuid);
            return action != null && bindingIndex >= 0 && bindingIndex < action.bindings.Count;
        }

        private void CompleteRebind(InputAction action, InputActionRebindingExtensions.RebindingOperation operation, TaskCompletionSource<bool> completion, bool result)
        {
            operation.Dispose();
            _rebindingOperation = null;
            action.Enable();
            if (result)
                SaveOverrides();
            completion.TrySetResult(result);
        }

        private void SaveOverrides()
        {
            if (_storage == null)
                return;

            GameSettingsData data = _storage.TryLoad(out GameSettingsData loaded) ? loaded : new GameSettingsData();
            data.BindingOverridesJson = SaveOverridesAsJson();
            _storage.Save(data);
        }
    }
}
