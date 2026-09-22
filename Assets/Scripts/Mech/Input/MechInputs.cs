using UnityEngine;
using UnityEngine.InputSystem;

public class MechInputs : MonoBehaviour
{
	private MechInputActions inputActions;

    // Reading the action directly means releasing a key immediately returns Vector2.zero.
    // We do not need performed/cancelled callbacks just to retain the current input value.
    public Vector2 MovementInput => inputActions == null
        ? Vector2.zero
        : inputActions.MechMovement.Move.ReadValue<Vector2>();

    public Vector2 CameraInput => inputActions == null
        ? Vector2.zero
        : inputActions.MechMovement.Camera.ReadValue<Vector2>();

   //mech actions

    public bool PrimaryActionPressedThisFrame =>
    inputActions != null &&
    inputActions.MechActions.PrimaryAction.WasPressedThisFrame();

    public bool PrimaryActionHeld =>
        inputActions != null &&
        inputActions.MechActions.PrimaryAction.IsPressed();

    public bool SecondaryActionPressedThisFrame =>
    inputActions != null &&
    inputActions.MechActions.SecondaryAction.WasPressedThisFrame();

    public bool SecondaryActionHeld =>
        inputActions != null &&
        inputActions.MechActions.SecondaryAction.IsPressed();

    //mech abilites

    public bool PrimaryAbilityPressedThisFrame =>
    inputActions != null &&
    inputActions.MechActions.PrimaryAbility.WasPressedThisFrame();

    public bool PrimaryAbilityHeld =>
        inputActions != null &&
        inputActions.MechActions.PrimaryAbility.IsPressed();

    public bool SecondaryAbilityPressedThisFrame =>
    inputActions != null &&
    inputActions.MechActions.SecondaryAbility.WasPressedThisFrame();

    public bool SecondaryAbilityHeld =>
        inputActions != null &&
        inputActions.MechActions.SecondaryAbility.IsPressed();

    public bool WasPressedThisFrame(MechActionSlot slot)
    {
        return slot switch
        {
            MechActionSlot.Primary => PrimaryActionPressedThisFrame,
            MechActionSlot.Secondary => SecondaryActionPressedThisFrame,
            MechActionSlot.PrimaryAbility => PrimaryAbilityPressedThisFrame,
            MechActionSlot.SecondaryAbility => SecondaryAbilityPressedThisFrame,
            _ => false
        };
    }

    public bool IsHeld(MechActionSlot slot)
    {
        return slot switch
        {
            MechActionSlot.Primary => PrimaryActionHeld,
            MechActionSlot.Secondary => SecondaryActionHeld,
            MechActionSlot.PrimaryAbility => PrimaryAbilityHeld,
            MechActionSlot.SecondaryAbility => SecondaryAbilityHeld,
            _ => false
        };
    }

    public void Initialize()
    {
        inputActions ??= new MechInputActions();
        inputActions.Enable();
    }

    public bool HasMovementInput(float deadZone = 0.01f)
    {
        return MovementInput.sqrMagnitude > deadZone * deadZone;
    }

    private void OnEnable()
    {
        inputActions?.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Disable();
    }

    private void OnDestroy()
    {
        inputActions?.Dispose();
    }
}
