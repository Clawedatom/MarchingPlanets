using UnityEngine;

public class PlanetBody : MonoBehaviour
{
    public Vector3 GetUp(Vector3 worldPos)
    {
        return (worldPos - transform.position).normalized;
    }
}
