using UnityEngine;

[CreateAssetMenu(menuName = "Marching Planets/Mech/Action Loadout")]
public class MechActionLoadout : ScriptableObject
{
    [SerializeField] private MechActionDefinition primary;
    [SerializeField] private MechActionDefinition secondary;
    [SerializeField] private MechActionDefinition primaryAbility;
    [SerializeField] private MechActionDefinition secondaryAbility;

    public MechActionDefinition Primary => primary;
    public MechActionDefinition Secondary => secondary;
    public MechActionDefinition PrimaryAbility => primaryAbility;
    public MechActionDefinition SecondaryAbility => secondaryAbility;
}
