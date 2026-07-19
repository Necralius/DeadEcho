using Project.Core.Services;
using UnityEngine;

namespace Project.Core.StateMachine.States
{
    public sealed class LoadingState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly IInputService _inputService;

        public LoadingState(AppStateMachine stateMachine, IInputService inputService = null)
        {
            _stateMachine = stateMachine;
            _inputService = inputService;
        }

        public AppStateId Id => AppStateId.Loading;

        public void Enter()
        {
            Debug.Log("[LoadingState] Enter - waiting for scene loading integration.");
            _inputService?.DisableGameplayInput();
        }

        public void Exit()
        {
            Debug.Log("[LoadingState] Exit.");
        }

        public void CompleteLoading()
        {
            Debug.Log("[LoadingState] Loading complete.");
            _stateMachine.ChangeState(AppStateId.Gameplay);
        }
    }
}
