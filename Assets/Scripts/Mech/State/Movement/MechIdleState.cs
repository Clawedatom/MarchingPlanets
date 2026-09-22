using UnityEngine;

public class MechIdleState : MechMovementState
{
    public MechIdleState(MechController mech) : base(mech) { }

    public override void Enter()
    {
        Mech.Motor.StopHorizontalMovement();
    }

    public override void Tick()
    {
        // Idle-specific animation / facing behaviour.
    }

    public override MechMovementState GetTransition()
    {
        if (Mech.HasMovementInput())
            return Mech.MovementStates.Moving;

        return null;
    }

    public override void Exit() { }
}
