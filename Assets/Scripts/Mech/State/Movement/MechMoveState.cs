public sealed class MechMoveState : MechMovementState
{
    public MechMoveState(MechController mech) : base(mech) { }

    public override void Enter() { }

    public override void Tick()
    {
        Mech.Motor.SetMovementInput(Mech.Input.MovementInput);
    }

    public override MechMovementState GetTransition()
    {
        if (!Mech.HasMovementInput())
            return Mech.MovementStates.Idle;

        return null;
    }

    public override void Exit() { }
}
