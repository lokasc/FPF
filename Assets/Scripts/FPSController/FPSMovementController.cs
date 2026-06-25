using UnityEngine;
using UnityEngine.InputSystem;
using FishNet.Object;

namespace FPS.Controller
{
    /// <summary>
    /// Root MonoBehaviour for the FPS character.
    ///
    /// Responsibilities:
    ///   - Owns the StateMachine and creates all state instances.
    ///   - Applies gravity and calls CharacterController.Move once per frame.
    ///   - Maintains shared flags (JumpRequested, SlideHeld) consumed by states.
    ///   - Exposes helpers for capsule resizing and TargetCameraLocalY.
    ///   - Directly binds to the PlayerInput component's InputActionAsset
    ///     (Player map) and exposes cooked values consumed by the controller
    ///     and states, replacing the former FPSInputReader ScriptableObject.
    ///
    /// Gravity is intentionally applied AFTER StateMachine.Tick() so that the
    /// upward impulse set in JumpState.Enter() is not clobbered in the same frame.
    ///
    /// Input is only subscribed for the owning client (IsOwner) via OnStartClient,
    /// ensuring non-owner proxies never process local input.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInput))]
    public class FPSMovementController : NetworkBehaviour
    {
        [Header("References")]
        [SerializeField] private FPSPlayerSettings _settings;
        private Player playerCore;
        public Timer dashTimer;
        
        [Header("Player Status")] 
        public bool canDash;

        public string currentState;
        
        // ── Polled values (formerly on FPSInputReader) ───────────────────────────
        // Updated each frame via InputAction callbacks on performed/canceled.
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        // ── Public context ───────────────────────────────────────────────────────
        public CharacterController CharController { get; private set; }
        public FPSPlayerSettings   Settings       => _settings;
        public StateMachine        StateMachine   { get; private set; }

        /// <summary>Full 3D velocity (units/s). States modify X/Z; this class manages Y.</summary>
        public Vector3 Velocity { get; set; }

        /// <summary>True while CharacterController reports ground contact this frame.</summary>
        public bool IsGrounded { get; private set; }

        /// <summary>
        /// Raised for one frame when the jump button is pressed.
        /// Cleared at the end of Update after the state machine has processed it.
        /// </summary>
        public bool JumpRequested { get; private set; }
        
        public bool DashRequested { get; private set; }

        /// <summary>True while the crouch/slide button is held.</summary>
        public bool SlideHeld { get; private set; }

        /// <summary>
        /// Camera local-Y target smoothly tracked by FPSCameraController.
        /// States write this on Enter/Exit.
        /// </summary>
        public float TargetCameraLocalY { get; set; }

        // ── State instances ──────────────────────────────────────────────────────
        public FPSIdleState  IdleState  { get; private set; }
        public FPSRunState   RunState   { get; private set; }
        public FPSSlideState SlideState { get; private set; }
        public FPSJumpState  JumpState  { get; private set; }
        public FPSDashState  DashState  { get; private set; }
        

        // ── PlayerInput & action references ─────────────────────────────────────
        // Actions are resolved from the InputActionAsset bound to the PlayerInput
        // component (Player map) rather than a separate ScriptableObject asset.
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _slideAction;
        private InputAction _dashAction;

        // ────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            CharController = GetComponent<CharacterController>();
            StateMachine   = new StateMachine();

            // Resolve the PlayerInput component and cache action references from
            // its bound asset (Assets/InputSystem_Actions.inputactions → Player map).
            _playerInput = GetComponent<PlayerInput>();
            playerCore = GetComponent<Player>();
            var map      = _playerInput.actions.FindActionMap("Player", throwIfNotFound: true);
            _moveAction  = map.FindAction("Move",   throwIfNotFound: true);
            _lookAction  = map.FindAction("Look",   throwIfNotFound: true);
            _jumpAction  = map.FindAction("Jump",   throwIfNotFound: true);
            _slideAction = map.FindAction("Crouch", throwIfNotFound: true);
            _dashAction = map.FindAction("Dash", throwIfNotFound: true);
            

            IdleState  = new FPSIdleState(this);
            RunState   = new FPSRunState(this);
            // SlideState = new FPSSlideState(this);
            JumpState  = new FPSJumpState(this);
            DashState = new FPSDashState(this);
            dashTimer = GetComponent<Timer>();
        }

        private void Start()
        {
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible   = false;

            TargetCameraLocalY    = _settings.NormalCameraLocalY;
            CharController.height = _settings.NormalHeight;
            // CharController.center = new Vector3(0f, _settings.NormalHeight * 0.5f, 0f);

            StateMachine.Initialize(IdleState);
        }

        /// <summary>
        /// Enable input and subscribe to actions only for the owning client.
        /// Non-owner proxies leave PlayerInput disabled and never bind callbacks,
        /// preventing ghost input from driving remote characters.
        /// </summary>
        public override void OnStartClient()
        {
            if (!IsOwner) return;
            _playerInput.enabled = true;
            SubscribeActions();
            
        }

        /// <summary>
        /// Mirror of OnStartClient — unsubscribe callbacks when the client stops
        /// to prevent stale delegates from firing after the object is despawned.
        /// </summary>
        public override void OnStopClient()
        {
            if (IsOwner)
                UnsubscribeActions();
        }

        // ── Action subscription helpers ──────────────────────────────────────────

        /// <summary>
        /// Binds all input callbacks. Called once for the owning client in
        /// OnStartClient. Kept in a dedicated method so OnStopClient can mirror
        /// it cleanly without duplicating delegate references.
        /// </summary>
        private void SubscribeActions()
        {
            _moveAction.performed  += HandleMove;
            _moveAction.canceled   += HandleMove;
            _lookAction.performed  += HandleLook;
            _lookAction.canceled   += HandleLook;
            _jumpAction.started    += HandleJump;
            _slideAction.started   += HandleSlideStarted;
            _slideAction.canceled  += HandleSlideEnded;
            _dashAction.performed += HandleDash;
        }

        /// <summary>
        /// Removes all input callbacks registered in SubscribeActions.
        /// Always call this before the object is destroyed or ownership changes
        /// to avoid memory leaks and phantom input processing.
        /// </summary>
        private void UnsubscribeActions()
        {
            _moveAction.performed  -= HandleMove;
            _moveAction.canceled   -= HandleMove;
            _lookAction.performed  -= HandleLook;
            _lookAction.canceled   -= HandleLook;
            _jumpAction.started    -= HandleJump;
            _slideAction.started   -= HandleSlideStarted;
            _slideAction.canceled  -= HandleSlideEnded;
            _dashAction.performed -= HandleDash;
            
            // Reset polled values so a disconnecting owner does not leave stale
            // directional input frozen on the controller.
            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
        }

        private void Update()
        {
            if (!IsOwner) return;

            IsGrounded = CharController.isGrounded;

            // Pin Y to a small negative value while grounded to maintain contact
            // without negative accumulation that would produce a visible stutter.
            if (IsGrounded && Velocity.y < 0f)
                Velocity = new Vector3(Velocity.x, -2f, Velocity.z);

            // States modify Velocity.x/z (and set Velocity.y on jump)
            StateMachine.Tick();

            // Gravity after the tick: a jump impulse written in Enter() this frame
            // survives untouched because IsGrounded is still true at that point.
            if (!IsGrounded)
                Velocity += Vector3.down * _settings.Gravity * Time.deltaTime;

            CharController.Move(Velocity * Time.deltaTime);

            // Consume jump flag — it is only valid for one state-machine tick
            JumpRequested = false;
            DashRequested = false;
            
            //Display current state in inspector
            currentState = StateMachine.CurrentState.ToString();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            StateMachine.FixedTick();
        }

        // ── Public helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Smoothly resizes the CharacterController capsule toward targetHeight.
        /// Centre is kept at half-height so the base stays at the character's feet.
        /// </summary>
        public void SetControllerHeight(float targetHeight)
        {
            // float next = Mathf.Lerp(CharController.height, targetHeight,
            //                         _settings.HeightSmoothSpeed * Time.deltaTime);
            // CharController.height = next;
            // CharController.center = new Vector3(0f, next * 0.5f, 0f);
        }

        // ── Handlers ─────────────────────────────────────────────────────────────

        // performed fires on any non-zero input; canceled fires when input returns
        // to zero. Handling both phases keeps MoveInput/LookInput accurate whether
        // the player is actively steering or has released the stick/keys.
        private void HandleMove(InputAction.CallbackContext ctx)
            => MoveInput = ctx.ReadValue<Vector2>();

        private void HandleLook(InputAction.CallbackContext ctx)
            => LookInput = ctx.ReadValue<Vector2>();

        // Jump uses started (not performed) so the impulse fires on the very first
        // frame the button is pressed.
        private void HandleJump(InputAction.CallbackContext ctx)
            => JumpRequested = true;

        private void HandleDash(InputAction.CallbackContext ctx)
        {
            if (canDash)
            {
                canDash = false;
                DashRequested = true;
                dashTimer.StartTimer(0.5f);
            }
        }
        
        // Slide tracks hold state: started → held, canceled → released.
        // States poll SlideHeld each tick rather than reacting to an event,
        // keeping slide logic self-contained inside FPSSlideState.
        private void HandleSlideStarted(InputAction.CallbackContext ctx)
            => SlideHeld = true;

        private void HandleSlideEnded(InputAction.CallbackContext ctx)
            => SlideHeld = false;

        public void OnDashTimerFinished()
        {
            canDash = true;
        }
    }
}