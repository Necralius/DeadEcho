using Project.Core.Services;
using UnityEngine;

namespace Project.Core.StateMachine.States
{
    public sealed class GameplayState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly IInputService _inputService;

        public GameplayState(AppStateMachine stateMachine, IInputService inputService = null)
        {
            _stateMachine = stateMachine;
            _inputService = inputService;
        }

        public AppStateId Id => AppStateId.Gameplay;

        public void Enter()
        {
            Debug.Log("[GameplayState] Enter - gameplay is active.");
            _inputService?.DisableUiInput();
            _inputService?.EnableGameplayInput();
        }

        public void Exit()
        {
            _inputService?.DisableGameplayInput();
            Debug.Log("[GameplayState] Exit.");
        }

        public void Pause()
        {
            Debug.Log("[GameplayState] Pause requested.");
            _stateMachine.ChangeState(AppStateId.Pause);
        }

        public void ReportPlayerDeath()
        {
            Debug.Log("[GameplayState] Player death reported.");
            _stateMachine.ChangeState(AppStateId.Death);
        }
    }
}
