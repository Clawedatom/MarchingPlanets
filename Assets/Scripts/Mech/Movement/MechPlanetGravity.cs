using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MechPlanetGravity : MonoBehaviour
{
    #region Class References
    private Rigidbody rb;
    #endregion

    #region Private Fields
    [SerializeField] private PlanetManager activePlanet; // for now manually assign to test
    [SerializeField] private float rotationAlignmentSpeed = 8f;
    #endregion

    #region Start Up
    public void OnAwake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable global physics gravity
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Prevent physics collisions from tipping mech
    }

    private void Awake()
    {
        OnAwake();
    }

    public void OnStart()
    {

    }
    #endregion

    #region Class Methods
    public void OnFixedUpdate()
    {
        if (activePlanet == null || activePlanet.Body == null) return;

        Vector3 planetUp = activePlanet.Body.GetUp(transform.position);

        // --- DEBUG RAYS ---
        Debug.DrawRay(transform.position, planetUp * 5f, Color.green);  // Planet Up
        Debug.DrawRay(transform.position, -planetUp * 5f, Color.red);   // Gravity Direction
        Debug.DrawRay(transform.position, transform.up * 5f, Color.blue); // Mech Local Up

        ApplyPlanetGravity(planetUp);
        AlignMechToPlanet(planetUp);
    }

    private void FixedUpdate()
    {
        OnFixedUpdate();
    }

    private void ApplyPlanetGravity(Vector3 planetUp)
    {
        Vector3 gravityForce = -planetUp * activePlanet.PlanetData.GravityStrength;
        rb.AddForce(gravityForce, ForceMode.Acceleration);
    }

    private void AlignMechToPlanet(Vector3 planetUp)
    {
        // Calculate target alignment relative to local Up
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, planetUp) * rb.rotation;
        
        // Slerp to target rotation smoothly
        Quaternion smoothedRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationAlignmentSpeed * Time.fixedDeltaTime);
        
        rb.MoveRotation(smoothedRotation);
    }

    private void OnDrawGizmosSelected()
    {
        if (activePlanet == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, activePlanet.transform.position);
    }
    #endregion
}