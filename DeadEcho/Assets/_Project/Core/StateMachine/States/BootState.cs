using Project.Core.Services;
using UnityEngine;

namespace Project.Core.StateMachine.States
{
    public sealed class BootState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly IInputService _inputService;

        public BootState(AppStateMachine stateMachine, IInputService inputService = null)
        {
            _stateMachine = stateMachine;
            _inputService = inputService;
        }

        public AppStateId Id => AppStateId.Boot;

        public void Enter()
        {
            Debug.Log("[BootState] Enter - initializing core services.");
            _inputService?.DisableGameplayInput();
            _inputService?.DisableUiInput();
            _stateMachine.ChangeState(AppStateId.MainMenu);
        }

        public void Exit()
        {
            Debug.Log("[BootState] Exit.");
        }
    }
}
