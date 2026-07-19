using Project.Core.Services;
using UnityEngine;

namespace Project.Core.StateMachine.States
{
    public sealed class DeathState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly IInputService _inputService;

        public DeathState(AppStateMachine stateMachine, IInputService inputService = null)
        {
            _stateMachine = stateMachine;
            _inputService = inputService;
        }

        public AppStateId Id => AppStateId.Death;

        public void Enter()
        {
            Debug.Log("[DeathState] Enter - player is dead.");
            _inputService?.DisableGameplayInput();
            _inputService?.EnableUiInput();
        }

        public void Exit()
        {
            Debug.Log("[DeathState] Exit.");
        }

        public void Restart()
        {
            Debug.Log("[DeathState] Restart requested.");
            _stateMachine.ChangeState(AppStateId.Loading);
        }
    }
}
