namespace Lowering.Iterators;

public class StateMachine(IState? startState)
{
    public IState? Current { get; private set; } = startState;

    public void Change(IState? state)
    {
        state = state ?? throw new ArgumentNullException(nameof(state));
        
        Current?.Exit();
        
        Current = state;
        
        Current.Enter();
    }

    public void Execute()
    {
        if(Current == null)
            throw new InvalidOperationException("No state");
        
        Current.Execute();
    }
}

public interface IState
{
    public void Enter();
    public void Execute();
    public void Exit();
}
