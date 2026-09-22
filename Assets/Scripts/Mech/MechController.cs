using UnityEngine;

[RequireComponent(typeof(MechMotor), typeof(MechInputs), typeof(Rigidbody))]
public class MechController : MonoBehaviour
{
    #region Class References
    [SerializeField] private MechMotor motor;
    [SerializeField] private MechStats stats;
    [SerializeField] private MechInputs input;
    [SerializeField] private MechEquipment equipment;
    [SerializeField] private MechPlanetGravity planetGravity;
    [SerializeField] private MechRaycasts raycasts;
    [SerializeField] private MechCamera camera;




    [Header("Debug Logging")]
    [Tooltip("Master switch. Disable this to silence every mech debug category.")]
    [SerializeField] private bool showDebugLogs;

    [Space(8)]
    [Header("Movement Debug")]
    [Tooltip("Logs when movement input starts or stops.")]
    [SerializeField] private bool logMovementInput = true;
    [Tooltip("Logs Idle / Moving transitions.")]
    [SerializeField] private bool logMovementStates = true;

    [Space(8)]
    [Header("Action Debug")]
    [Tooltip("Logs which action slot was pressed and whether it has an equipped action.")]
    [SerializeField] private bool logActionInput = true;
    [Tooltip("Logs entering, holding, releasing, and leaving action states.")]
    [SerializeField] private bool logActionStates = true;

    [Space(8)]
    [Header("Equipment and Tool Debug")]
    [Tooltip("Logs loadout changes and instantiated equipped actions.")]
    [SerializeField] private bool logEquipment = true;
    [Tooltip("For individual tools, such as mining rays, hit results, VFX, and terrain edits.")]
    [SerializeField] private bool logToolEvents = true;
    private bool hadMovementInput;

    #endregion

    #region Private Fields
    [Header("State Machines")]
    // State machine for the mech's movement
    public StateMachine<MechMovementState> MovementStateMachine { get; private set; }
    public StateMachine<MechActionState> ActionStateMachine { get; private set; }
    #endregion
    #region Properties
    public MechMotor Motor => motor;
    public MechStats Stats => stats;
    public MechInputs Input => input;
    public MechEquipment Equipment => equipment;
    public MechMovementStateSet MovementStates { get; private set; }
    public MechActionStateSet ActionStates { get; private set; }
    public MechPlanetGravity PlanetGravity => planetGravity;
    public MechRaycasts Raycasts => raycasts;
    public MechCamera Camera => camera;
    public bool ShowDebugLogs => showDebugLogs;

    public bool IsInActionState => ActionStateMachine.CurrentState != ActionStates.None;
    #endregion

    #region Start Up
    private void Awake()
    {
        motor ??= GetComponent<MechMotor>();
        stats ??= GetComponent<MechStats>();
        input ??= GetComponent<MechInputs>();
        equipment ??= GetComponent<MechEquipment>();
        planetGravity ??= GetComponent<MechPlanetGravity>();
        raycasts = GetComponent<MechRaycasts>();
        camera = GetComponentInChildren<MechCamera>();
        if (motor == null || input == null || GetComponent<Rigidbody>() == null)
        {
            Debug.LogError("MechController requires MechMotor, MechInputs, and Rigidbody components.", this);
            enabled = false;
            return;
        }

        motor.OnAwake();
        equipment?.Initialize(this);
        input.Initialize();
        planetGravity?.OnAwake();
        camera.OnAwake();

        MovementStates = new MechMovementStateSet(this);
        ActionStates = new MechActionStateSet(this);
        MovementStateMachine = new StateMachine<MechMovementState>();
        ActionStateMachine = new StateMachine<MechActionState>();
        MovementStateMachine.SetInitialState(MovementStates.Idle);
        ActionStateMachine.SetInitialState(ActionStates.None);
        LogDebug("Initialised. Movement=MechIdleState, Action=MechNoActionState.");
    }

    private void Start()
    {
        Motor.OnStart();
        Stats?.OnStart();
        planetGravity?.OnStart();
        Camera.OnStart();
    }
    #endregion

    #region Update Methods
    private void Update()
    {
        raycasts.OnUpdate();
        LogMovementInputEdge();
        UpdateMovementStateMachine();
        UpdateActionStateMachine();
        Camera.OnUpdate();
    }

    private void FixedUpdate()
    {
        planetGravity?.OnFixedUpdate();

    }

    private void LateUpdate()
    {
        camera.OnLateUpdate();

    }
    #endregion

    #region Input Methods
    public bool HasMovementInput()
    {
        return input != null && input.HasMovementInput();
    }

    #endregion

    #region Debug Mehtods
    public void LogDebug(string message)
    {
        Log(message, true);
    }

    public void LogMovementInputDebug(string message)
    {
        Log(message, logMovementInput);
    }

    public void LogMovementStateDebug(string message)
    {
        Log(message, logMovementStates);
    }

    public void LogActionInputDebug(string message)
    {
        Log(message, logActionInput);
    }

    public void LogActionStateDebug(string message)
    {
        Log(message, logActionStates);
    }

    public void LogEquipmentDebug(string message)
    {
        Log(message, logEquipment);
    }
    public void LogToolDebug(string message)
    {
        Log(message, logToolEvents);
    }

    private void Log(string message, bool categoryEnabled)
    {
        if (showDebugLogs && categoryEnabled)
        {
            Debug.Log($"[Mech: {name}] {message}", this);
        }
    }

    private void LogMovementInputEdge()
    {
        bool hasMovementInput = HasMovementInput();
        if (hasMovementInput == hadMovementInput)
            return;

        hadMovementInput = hasMovementInput;
        string eventName = hasMovementInput ? "started" : "stopped";
        LogMovementInputDebug($"Movement input {eventName}. Value: {input.MovementInput}");
    }

    private void LogStateTransition(string machineName, IState current, IState next)
    {
        if (next != null && !ReferenceEquals(current, next))
        {
            string message = $"{machineName} state: {current.GetType().Name} -> {next.GetType().Name}";

            if (machineName == "Movement")
                LogMovementStateDebug(message);
            else
                LogActionStateDebug(message);
        }
    }
    #endregion

    #region State Machine Methods
    private void UpdateMovementStateMachine()
    {
        MovementStateMachine.Tick();

        MechMovementState current = MovementStateMachine.CurrentState;
        MechMovementState next = current.GetTransition();
        LogStateTransition("Movement", current, next);
        MovementStateMachine.ChangeState(next);
    }

    private void UpdateActionStateMachine()
    {


        ActionStateMachine.Tick();

        MechActionState current = ActionStateMachine.CurrentState;
        MechActionState next = current.GetTransition();
        LogStateTransition("Action", current, next);
        ActionStateMachine.ChangeState(next);
    }
    #endregion









}

