namespace Project.Core.StateMachine
{
    public interface IAppState
    {
        AppStateId Id { get; }

        void Enter();
        void Exit();
    }
}
