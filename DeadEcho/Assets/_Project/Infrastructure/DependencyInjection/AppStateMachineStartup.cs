using Project.Core.StateMachine;
using VContainer.Unity;

namespace Project.Infrastructure.DependencyInjection
{
    public sealed class AppStateMachineStartup : IStartable
    {
        private readonly AppStateMachine _stateMachine;

        public AppStateMachineStartup(AppStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public void Start()
        {
            _stateMachine.ChangeState(AppStateId.Boot);
        }
    }
}
