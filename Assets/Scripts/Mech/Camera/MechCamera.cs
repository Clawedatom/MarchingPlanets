using UnityEngine;

public class MechCamera : MonoBehaviour
{
    #region Class Methods
    MechController mech;
    Camera camera;
    #endregion

    #region Private Fields

    [Header("Look Settings")]
    [SerializeField, Min(0.01f)] private float sensitivity = 0.12f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-70f, 70f);

    [Header("Mech Weight")]
    [SerializeField, Min(0f)] private float rotationDelay = 0.08f;

    private float targetPitch;
    private float currentPitch;
    private float pitchVelocity;

    private float targetYawDelta;
    private float currentYawDelta;
    private float yawVelocity;

    [Header("Indicator Fields")]
    [SerializeField] private GameObject targetIndicatorPrefab;
    [SerializeField] private float indicatorRayDist = 10f;
    [SerializeField] private LayerMask ignoreIndicatorLayer;

    [SerializeField] private float noActionAlpha = 0.25f;
    [SerializeField] private float actionAlpha = 1f;

    [SerializeField] private bool hasIndicatorTarget;

    private GameObject indicatorGO;
    private MeshRenderer indicatorMeshRenderer;
    #endregion

    #region Properties
    public Camera GetCamera => camera;

    #endregion

    #region Start Up
    public void OnAwake()
    {
        mech ??= GetComponentInParent<MechController>();

        if (mech == null)
        {
            Debug.LogError("MechCamera must be a child of, or reference, a MechController.", this);
            enabled = false;
            return;
        }
        camera = GetComponentInChildren<Camera>();
        targetPitch = currentPitch = NormalizeAngle(transform.localEulerAngles.x);
    }

    public void OnStart()
    {
        SetUpTargetIndicator();
    }

    private void SetUpTargetIndicator()
    {
        indicatorGO = Instantiate(targetIndicatorPrefab, transform.position, Quaternion.identity, this.transform);
        indicatorMeshRenderer = indicatorGO.GetComponent<MeshRenderer>();

    }
    #endregion

    #region Class Methods

    public void OnUpdate()
    {
        //fire raycast, if hit, show indicator on hit point, if not hide indicator. set indicator alpha ~25%/100% if doing action.



        MechHitInfo hitInfo;

        if (CheckIndicatorRay(out hitInfo))
        {
            MoveIndicatorTarget(hitInfo);
        }
        else
        {
            if (indicatorGO.activeSelf != false)
            {
                indicatorGO.SetActive(false);
            }
        }



    }

    private void MoveIndicatorTarget(MechHitInfo hitInfo)
    {
        if (indicatorGO.activeSelf == false)
        {
            indicatorGO.SetActive(true);
        }

        indicatorGO.transform.position = hitInfo.Point;
        indicatorGO.transform.rotation = Quaternion.FromToRotation(Vector3.up,hitInfo.Normal);

        float newAlpha = mech.IsInActionState ? actionAlpha : noActionAlpha;

        Color col = indicatorMeshRenderer.sharedMaterial.color;

        col.a = newAlpha;
        indicatorMeshRenderer.sharedMaterial.color = col;
    }

    private bool CheckIndicatorRay(out MechHitInfo hitInfo)
    {
        hitInfo = mech.Raycasts.FireCast(
            GetCamera.transform.position,
            GetCamera.transform.forward,
            indicatorRayDist,
            ~ignoreIndicatorLayer,
            "IndicatorRay",
            Color.blanchedAlmond
        );



        
        return hitInfo.DidHit;
    }
    public void OnLateUpdate()
    {
        if (mech == null || mech.Input == null) return;

        Vector2 lookInput = mech.Input.CameraInput;

        // 1. Camera Pitch (Pitch only affects the camera pivot locally)
        targetPitch = Mathf.Clamp(targetPitch - lookInput.y * sensitivity, pitchLimits.x, pitchLimits.y);
        currentPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, rotationDelay);
        transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

        // 2. Mech Yaw (Rotate mech around its local Up vector, preserving planet alignment)
        targetYawDelta = lookInput.x * sensitivity;
        currentYawDelta = Mathf.SmoothDamp(currentYawDelta, targetYawDelta, ref yawVelocity, rotationDelay);

        mech.transform.Rotate(Vector3.up, currentYawDelta, Space.Self);
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
    #endregion

    #region Indicator Methods
    
    #endregion




}