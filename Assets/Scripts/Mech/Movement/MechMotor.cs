using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MechMotor : MonoBehaviour
{
    #region Class References
    private Rigidbody rb;
    #endregion

    #region Private Fields
    [Header("Movement Speed")]
    [SerializeField] private float moveSpeed = 5f;
    private Vector2 movementInput;
    #endregion

    #region Start Up
    public void OnAwake()
    {
        rb = GetComponent<Rigidbody>();
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
    public void SetMovementInput(Vector2 input)
    {
        movementInput = Vector2.ClampMagnitude(input, 1f);
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        // Calculate surface movement direction using local Right and Forward (already aligned to planet tangent)
        Vector3 moveDirection = (transform.right * movementInput.x + transform.forward * movementInput.y);
        Vector3 targetSurfaceVelocity = moveDirection * moveSpeed;

        // Extract current vertical velocity parallel to the local Up (gravity direction)
        Vector3 gravityVelocity = Vector3.Project(rb.linearVelocity, transform.up);

        // Combine surface movement with preserved gravity velocity
        rb.linearVelocity = targetSurfaceVelocity + gravityVelocity;
    }

    public void StopHorizontalMovement()
    {
        movementInput = Vector2.zero;
        if (rb == null) return;

        // Preserve gravity velocity while clearing surface velocity
        rb.linearVelocity = Vector3.Project(rb.linearVelocity, transform.up);
    }
    #endregion
}