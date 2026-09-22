public sealed class MechActionStateSet
{
    public MechNoActionState None { get; }
    public UsingActionState Using { get; }

    public MechActionStateSet(MechController mech)
    {
        None = new MechNoActionState(mech);
        Using = new UsingActionState(mech);
    }
}
