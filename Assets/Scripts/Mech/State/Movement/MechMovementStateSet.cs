/// <summary>
/// Owns the movement-state instances for one mech. States are created once and
/// returned by transitions, so changing state does not allocate garbage.
/// </summary>
public sealed class MechMovementStateSet
{
    public MechIdleState Idle { get; }
    public MechMoveState Moving { get; }

    public MechMovementStateSet(MechController mech)
    {
        Idle = new MechIdleState(mech);
        Moving = new MechMoveState(mech);
    }
}
