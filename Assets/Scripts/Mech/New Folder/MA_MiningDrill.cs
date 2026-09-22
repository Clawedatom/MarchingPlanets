using UnityEngine;

public class MA_MiningDrill : MechAction
{
    [SerializeField] private LayerMask drillLayer;  
    [SerializeField] private float drillRadius = 5f;

    protected override void OnInitialized()
    {
        Mech.LogToolDebug("Mining drill initialised.");
    }

    public override void OnEquip(MechController mech)
    {
        mech.LogToolDebug("Mining drill equipped.");
    }

    public override void Begin(MechController mech)
    {
        mech.LogToolDebug("Mining drill started.");
        // Start drill animation, VFX, and audio here.
    }

    public override void Tick(MechController mech)
    {
        Camera cam = mech.Camera.GetCamera;

        MechHitInfo hitInfo = mech.Raycasts.FireCast(
            cam.transform.position,
            cam.transform.forward,
            10f,
            drillLayer,
            "MiningDrill"
        );

        if (mech.ShowDebugLogs)
        {
            Debug.Log(hitInfo.DidHit ? "Drilling - Hit" : "Drilling - Missing");
        }

        if (hitInfo.DidHit)
        {
            if (hitInfo.TryGetComponent<PlanetChunk>(out PlanetChunk chunk))
            {
                chunk.Dig(hitInfo.Point, hitInfo.Normal, drillRadius);
            }
        }
    }

    public override void End(MechController mech)
    {
        mech.LogToolDebug("Mining drill stopped.");
        // Stop drill animation, VFX, and audio here.
    }
}
