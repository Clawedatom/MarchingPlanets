/// <summary>
/// The action machine begins here. Later this state will transition to the
/// action represented by the pressed slot in MechEquipment.CurrentLoadout.
/// </summary>
public sealed class MechNoActionState : MechActionState
{
    public MechNoActionState(MechController mech) : base(mech) { }

    public override void Enter() { }
    public override void Tick() { }
    public override void Exit() { }
    public override MechActionState GetTransition()
    {
        return TryStartAction(MechActionSlot.Primary)
            ?? TryStartAction(MechActionSlot.Secondary)
            ?? TryStartAction(MechActionSlot.PrimaryAbility)
            ?? TryStartAction(MechActionSlot.SecondaryAbility);
    }

    private MechActionState TryStartAction(MechActionSlot slot)
    {
        if (!Mech.Input.WasPressedThisFrame(slot))
            return null;

        MechAction action = Mech.Equipment != null
            ? Mech.Equipment.GetEquippedAction(slot)
            : null;

        if (action == null)
        {
            Mech.LogActionInputDebug($"{slot} was pressed, but no action is equipped in that slot.");
            return null;
        }

        if (!action.CanStart(Mech))
        {
            Mech.LogActionInputDebug($"{action.GetType().Name} rejected its {slot} start request.");
            return null;
        }

        Mech.LogActionInputDebug($"{slot} pressed. Preparing {action.GetType().Name}.");
        return Mech.ActionStates.Using.Configure(action, slot);
    }

}
