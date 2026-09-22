using UnityEngine;
using System.Collections.Generic;

public enum MechActionSlot
{
    Primary,
    Secondary,
    PrimaryAbility,
    SecondaryAbility
}

public class MechEquipment : MonoBehaviour
{
    [SerializeField] private MechActionLoadout miningLoadout;
    [SerializeField] private MechActionLoadout combatLoadout;
    [SerializeField] private Transform actionMount;

    private readonly Dictionary<MechActionSlot, MechAction> equippedActions = new();
    private MechController mech;
    public MechActionLoadout CurrentLoadout { get; private set; }

    public void Initialize(MechController owner)
    {
        mech = owner;
        actionMount ??= transform;
        EquipLoadout(miningLoadout);
    }

    public MechAction GetEquippedAction(MechActionSlot slot)
    {
        return equippedActions.TryGetValue(slot, out MechAction action) ? action : null;
    }

    public void EquipMiningLoadout() => EquipLoadout(miningLoadout);
    public void EquipCombatLoadout() => EquipLoadout(combatLoadout);

    private void EquipLoadout(MechActionLoadout loadout)
    {
        ClearEquippedActions();
        CurrentLoadout = loadout;

        if (loadout == null)
        {
            mech?.LogEquipmentDebug("No action loadout is equipped.");
            return;
        }

        CreateAction(MechActionSlot.Primary, loadout.Primary);
        CreateAction(MechActionSlot.Secondary, loadout.Secondary);
        CreateAction(MechActionSlot.PrimaryAbility, loadout.PrimaryAbility);
        CreateAction(MechActionSlot.SecondaryAbility, loadout.SecondaryAbility);
        mech?.LogEquipmentDebug($"Equipped loadout: {loadout.name}");
    }

    private void CreateAction(MechActionSlot slot, MechActionDefinition definition)
    {
        if (definition == null || definition.ActionPrefab == null)
            return;

        MechAction action = Instantiate(definition.ActionPrefab, actionMount);
        action.Initialize(mech, definition);
        action.OnEquip(mech);
        equippedActions.Add(slot, action);
        mech?.LogEquipmentDebug($"Equipped {slot}: {definition.DisplayName}.");
    }

    private void ClearEquippedActions()
    {
        foreach (MechAction action in equippedActions.Values)
        {
            if (action != null)
                Destroy(action.gameObject);
        }

        equippedActions.Clear();
    }
}
