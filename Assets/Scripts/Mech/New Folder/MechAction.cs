using UnityEngine;

/// <summary>
/// Runtime behaviour for one equipped action. A MechActionDefinition owns the
/// reusable configuration; this component owns the live gameplay behaviour.
/// </summary>
public abstract class MechAction : MonoBehaviour
{
    public virtual bool RequiresHold => true;

    protected MechController Mech { get; private set; }
    protected MechActionDefinition Definition { get; private set; }

    public void Initialize(MechController mech, MechActionDefinition definition)
    {
        Mech = mech;
        Definition = definition;
        OnInitialized();
    }

    public virtual bool CanStart(MechController mech) => true;

    protected virtual void OnInitialized() { }
    public abstract void OnEquip(MechController mech);
    public abstract void Begin(MechController mech);
    public abstract void Tick(MechController mech);
    public abstract void End(MechController mech);
}
