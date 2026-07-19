using Project.Core.Services;
using UnityEngine;

namespace Project.Core.StateMachine.States
{
    public sealed class PauseState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly IInputService _inputService;
        private float _previousTimeScale = 1f;

        public PauseState(AppStateMachine stateMachine, IInputService inputService = null)
        {
            _stateMachine = stateMachine;
            _inputService = inputService;
        }

        public AppStateId Id => AppStateId.Pause;

        public void Enter()
        {
            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _inputService?.DisableGameplayInput();
            _inputService?.EnableUiInput();
            Debug.Log("[PauseState] Enter - game paused.");
        }

        public void Exit()
        {
            Time.timeScale = _previousTimeScale;
            _inputService?.DisableUiInput();
            Debug.Log("[PauseState] Exit - game resumed.");
        }

        public void Resume()
        {
            Debug.Log("[PauseState] Resume requested.");
            _stateMachine.ChangeState(AppStateId.Gameplay);
        }
    }
}
