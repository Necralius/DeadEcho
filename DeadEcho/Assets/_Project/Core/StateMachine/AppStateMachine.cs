using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Core.StateMachine
{
    public sealed class AppStateMachine
    {
        private readonly Dictionary<AppStateId, IAppState> _states = new();

        public AppStateId? CurrentStateId { get; private set; }
        public IAppState CurrentState { get; private set; }

        public void RegisterState(IAppState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            if (_states.ContainsKey(state.Id))
                throw new InvalidOperationException($"App state '{state.Id}' is already registered.");

            _states.Add(state.Id, state);
        }

        public void ChangeState(AppStateId nextStateId)
        {
            if (CurrentStateId == nextStateId)
            {
                Debug.Log($"[AppStateMachine] Ignored duplicate transition to '{nextStateId}'.");
                return;
            }

            if (!_states.TryGetValue(nextStateId, out IAppState nextState))
                throw new InvalidOperationException($"App state '{nextStateId}' is not registered.");

            AppStateId? previousStateId = CurrentStateId;
            IAppState previousState = CurrentState;

            Debug.Log(previousStateId.HasValue
                ? $"[AppStateMachine] Transition: {previousStateId.Value} -> {nextStateId}"
                : $"[AppStateMachine] Entering initial state: {nextStateId}");

            previousState?.Exit();

            CurrentState = nextState;
            CurrentStateId = nextStateId;

            nextState.Enter();
        }

        public bool IsStateRegistered(AppStateId stateId) => _states.ContainsKey(stateId);
    }
}
