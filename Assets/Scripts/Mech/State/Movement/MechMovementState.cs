using UnityEngine;

public abstract class MechMovementState : IState
{
    protected readonly MechController Mech;

    protected MechMovementState(MechController mech)
    {
        Mech = mech;
    }
    public abstract void Enter();
    public abstract void Tick();

    public abstract void Exit();

    // Null means: remain in this state.
    public abstract MechMovementState GetTransition();

}
