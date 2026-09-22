using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Marching Planets/Mech/Action Definition")]
public class MechActionDefinition : ScriptableObject
{
    [Header("Presentation")]
    [SerializeField] private string displayName;
    [TextArea][SerializeField] private string description;
    [SerializeField] private string animationTrigger;

    [Header("Runtime")]
    [Tooltip("A prefab with a MechAction-derived component, such as MA_MiningDrill.")]
    [SerializeField] private MechAction actionPrefab;

    [Header("Costs")]
    [SerializeField] private float heatPerSecond;

    public string DisplayName => displayName;
    public string Description => description;
    public string AnimationTrigger => animationTrigger;
    public MechAction ActionPrefab => actionPrefab;
    public float HeatPerSecond => heatPerSecond;

}
