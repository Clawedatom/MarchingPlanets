using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detailed hit outcome returned by MechRaycasts.
/// </summary>
public struct MechHitInfo
{
    public bool DidHit;
    public RaycastHit RawHit;

    public Vector3 Point => RawHit.point;
    public Vector3 Normal => RawHit.normal;
    public float Distance => RawHit.distance;
    public Collider Collider => RawHit.collider;
    public Transform Transform => RawHit.transform;
    public GameObject GameObject => RawHit.collider != null ? RawHit.collider.gameObject : null;
    public Rigidbody Rigidbody => RawHit.rigidbody;
    public int TriangleIndex => RawHit.triangleIndex;

    /// <summary>
    /// Helper to safely retrieve a component attached to the hit object.
    /// </summary>
    public bool TryGetComponent<T>(out T component) where T : class
    {
        if (Collider != null)
        {
            return Collider.TryGetComponent(out component);
        }
        component = null;
        return false;
    }

    /// <summary>
    /// Helper to check if the hit object has a specific tag.
    /// </summary>
    public bool CompareTag(string tag)
    {
        return GameObject != null && GameObject.CompareTag(tag);
    }
}

/// <summary>
/// Container holding raycast debug data and stored object references.
/// </summary>
[System.Serializable]

public class DebugRay
{
    public string RayType;
    public Vector3 Origin;
    public Vector3 Direction;
    public float Distance;

    public bool IsHit;
    public Vector3 HitPoint;
    public Vector3 HitNormal;

    public float TimeCreated;
    public float Lifetime;
    public Color Color;

    public Collider HitCollider;
    public GameObject HitGameObject;
    public float HitDistance;

    public DebugRay(
        string rayType,
        Vector3 origin,
        Vector3 direction,
        float distance,
        bool isHit,
        RaycastHit hit,
        Color color,
        float lifetime)
    {
        RayType = rayType;
        Origin = origin;
        Direction = direction;
        Distance = distance;

        IsHit = isHit;
        TimeCreated = Time.time;
        Lifetime = lifetime;
        Color = color;

        if (isHit)
        {
            HitPoint = hit.point;
            HitNormal = hit.normal;
            HitCollider = hit.collider;
            HitGameObject = hit.collider != null
                ? hit.collider.gameObject
                : null;

            HitDistance = hit.distance;
        }
        else
        {
            HitPoint = origin + direction * distance;
            HitNormal = Vector3.zero;
            HitCollider = null;
            HitGameObject = null;
            HitDistance = distance;
        }
    }
}

public class MechRaycasts : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebug = true;
    [SerializeField] private float debugDuration = 0.2f;
    [SerializeField] private Color hitColor = Color.green;
    [SerializeField] private Color missColor = Color.red;
    [SerializeField] private Color normalColor = Color.cyan;

    [SerializeField]private  List<DebugRay> activeDebugRays = new List<DebugRay>();

    public void OnUpdate()
    {
        if (activeDebugRays.Count > 0)
        {
            activeDebugRays.RemoveAll(r => Time.time - r.TimeCreated > r.Lifetime);
        }
    }

    /// <summary>
    /// Fires a raycast returning a detailed MechHitInfo object.
    /// </summary>
    public MechHitInfo FireCast(Vector3 origin, Vector3 direction, float range, LayerMask layerMask, string rayType = "General", Color? debugColor = null, float debugLifetime = 0.2f)
    {
        Vector3 normalizedDir = direction.normalized;
        bool isHit = Physics.Raycast(origin, normalizedDir, out RaycastHit hit, range, layerMask);
        Color rayColour = debugColor ?? (isHit ? hitColor : missColor);

        if (enableDebug)
        {
            DebugRay rayData = new DebugRay(
                rayType,
                origin,
                normalizedDir,
                range,
                isHit,
                hit,
                rayColour,
                debugLifetime
            );

            activeDebugRays.Add(rayData);

            Vector3 debugEndpoint =
                isHit
                    ? hit.point
                    : origin + normalizedDir * range;

            Debug.DrawLine(
                origin,
                debugEndpoint,
                rayColour,
                debugLifetime
            );

            if (isHit)
            {
                Debug.DrawRay(
                    hit.point,
                    hit.normal * 0.5f,
                    normalColor,
                    debugLifetime
                );
            }
        }

        return new MechHitInfo
        {
            DidHit = isHit,
            RawHit = hit
        };
    }

    /// <summary>
    /// Overload returning standard out RaycastHit alongside MechHitInfo.
    /// </summary>
    public bool FireCast(Vector3 origin, Vector3 direction, out RaycastHit hit, float range, LayerMask layerMask, string rayType = "General", Color? debugColor = null, float debugLifetime = 0.2f)
    {
        MechHitInfo info = FireCast(origin, direction, range, layerMask, rayType, debugColor, debugLifetime);
        hit = info.RawHit;
        return info.DidHit;
    }

    /// <summary>
    /// Overload firing directly from a Transform using MechHitInfo.
    /// </summary>
    public MechHitInfo FireCast(Transform source, float range, LayerMask layerMask, string rayType = "General", Color? debugColor = null, float debugLifetime = 0.2f)
    {
        return FireCast(source.position, source.forward, range, layerMask, rayType, debugColor, debugLifetime);
    }

    /// <summary>
    /// Overload firing from a Transform with standard out RaycastHit.
    /// </summary>
    public bool FireCast(Transform source, out RaycastHit hit, float range, LayerMask layerMask, string rayType = "General", Color? debugColor = null, float debugLifetime = 0.2f)
    {
        return FireCast(source.position, source.forward, out hit, range, layerMask, rayType, debugColor, debugLifetime);
    }

    public IReadOnlyList<DebugRay> GetActiveDebugRays() => activeDebugRays.AsReadOnly();

    private void OnDrawGizmos()
    {
        if (!enableDebug || !Application.isPlaying)
            return;

        foreach (DebugRay ray in activeDebugRays)
        {
            Gizmos.color = ray.Color;

            Vector3 endpoint = ray.IsHit
                ? ray.HitPoint
                : ray.Origin + ray.Direction * ray.Distance;

            Gizmos.DrawLine(ray.Origin, endpoint);

            if (ray.IsHit)
            {
                Gizmos.DrawSphere(ray.HitPoint, 0.08f);

                Gizmos.DrawRay(
                    ray.HitPoint,
                    ray.HitNormal * 0.4f
                );
            }
        }
    }
}