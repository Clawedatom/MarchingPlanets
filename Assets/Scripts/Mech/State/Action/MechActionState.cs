public abstract class MechActionState : IState
{
    protected readonly MechController Mech;

    protected MechActionState(MechController mech)
    {
        Mech = mech;
    }

    public abstract void Enter();
    public abstract void Tick();
    public abstract void Exit();
    public abstract MechActionState GetTransition();
}
