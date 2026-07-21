using Project.Core.StateMachine.States;
using Project.Core.Services;

namespace Project.Core.StateMachine
{
    public static class AppStateMachineFactory
    {
        public static AppStateMachine CreateDefault(IInputService inputService = null, IUiService uiService = null)
        {
            var stateMachine = new AppStateMachine();

            stateMachine.RegisterState(new BootState(stateMachine, inputService));
            stateMachine.RegisterState(new MainMenuState(stateMachine, inputService, uiService));
            stateMachine.RegisterState(new LoadingState(stateMachine, inputService));
            stateMachine.RegisterState(new GameplayState(stateMachine, inputService));
            stateMachine.RegisterState(new PauseState(stateMachine, inputService));
            stateMachine.RegisterState(new DeathState(stateMachine, inputService));

            return stateMachine;
        }
    }
}
