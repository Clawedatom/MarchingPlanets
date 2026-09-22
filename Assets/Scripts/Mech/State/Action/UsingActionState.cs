/// <summary>
/// One reusable state for all actions. Configure supplies the live action and
/// its input slot immediately before the state is entered.
/// </summary>
public sealed class UsingActionState : MechActionState
{
    private MechAction activeAction;
    private MechActionSlot activeSlot;
    private bool hasLoggedHeld;

    public UsingActionState(MechController mech) : base(mech)
    {
    }

    public UsingActionState Configure(MechAction action, MechActionSlot slot)
    {
        activeAction = action;
        activeSlot = slot;
        return this;
    }

    public override void Enter()
    {
        hasLoggedHeld = false;
        Mech.LogActionStateDebug($"Entered action state: {activeAction.GetType().Name} ({activeSlot}).");
        activeAction.Begin(Mech);
    }

    public override void Exit()
    {
        if (activeAction != null)
        {
            activeAction.End(Mech);
            Mech.LogActionStateDebug($"Left action state: {activeAction.GetType().Name} ({activeSlot}).");
        }

        activeAction = null;
    }

    public override MechActionState GetTransition()
    {
        if (activeAction == null)
            return Mech.ActionStates.None;

        if (activeAction.RequiresHold && !Mech.Input.IsHeld(activeSlot))
        {
            Mech.LogActionStateDebug($"{activeSlot} released. Ending {activeAction.GetType().Name}.");
            return Mech.ActionStates.None;
        }

        return null;
    }

    public override void Tick()
    {
        if (activeAction == null)
            return;

        activeAction.Tick(Mech);

        if (!hasLoggedHeld && Mech.Input.IsHeld(activeSlot))
        {
            hasLoggedHeld = true;
            Mech.LogActionStateDebug($"Holding {activeSlot}: {activeAction.GetType().Name}.");
        }
    }

  
}
