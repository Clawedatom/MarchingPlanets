public sealed class StateMachine<TState> where TState : class, IState
{
    public TState CurrentState { get; private set; }

    public void SetInitialState(TState initialState)
    {
        if (initialState == null)
        {
            throw new System.ArgumentNullException(nameof(initialState));
        }

        CurrentState?.Exit();
        CurrentState = initialState;
        CurrentState.Enter();
    }

    public void ChangeState(TState nextState)
    {
        if (nextState == null || ReferenceEquals(CurrentState, nextState))
        {
            return;
        }
        CurrentState?.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
    }

    public void Tick()
    {
        CurrentState?.Tick();
    }
}

public interface IState
{
    void Enter();
    void Tick();
    void Exit();
    
}
