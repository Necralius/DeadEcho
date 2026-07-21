using Project.Core.Services;
using UnityEngine;

namespace Project.Core.StateMachine.States
{
    public sealed class MainMenuState : IAppState
    {
        private readonly AppStateMachine _stateMachine;
        private readonly IInputService _inputService;
        private readonly IUiService _uiService;

        public MainMenuState(AppStateMachine stateMachine, IInputService inputService = null, IUiService uiService = null)
        {
            _stateMachine = stateMachine;
            _inputService = inputService;
            _uiService = uiService;
        }

        public AppStateId Id => AppStateId.MainMenu;

        public void Enter()
        {
            Debug.Log("[MainMenuState] Enter - main menu is active.");
            _inputService?.DisableGameplayInput();
            _inputService?.EnableUiInput();
            _uiService?.Replace(UiScreenId.MainMenu);
        }

        public void Exit()
        {
            _uiService?.CloseCurrent();
            Debug.Log("[MainMenuState] Exit.");
        }

        public void StartGame()
        {
            Debug.Log("[MainMenuState] Start game requested.");
            _stateMachine.ChangeState(AppStateId.Loading);
        }
    }
}
