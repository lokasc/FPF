using UnityEngine;

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
    ///
    /// Gravity is intentionally applied AFTER StateMachine.Tick() so that the
    /// upward impulse set in JumpState.Enter() is not clobbered in the same frame.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FPSMovementController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FPSInputReader    _input;
        [SerializeField] private FPSPlayerSettings _settings;

        // ── Public context ───────────────────────────────────────────────────────
        public CharacterController CharController { get; private set; }
        public FPSInputReader      Input          => _input;
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

        // ────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            CharController = GetComponent<CharacterController>();
            StateMachine   = new StateMachine();

            IdleState  = new FPSIdleState(this);
            RunState   = new FPSRunState(this);
            SlideState = new FPSSlideState(this);
            JumpState  = new FPSJumpState(this);
        }

        private void Start()
        {
            // Cursor.lockState = CursorLockMode.Locked;
            // Cursor.visible   = false;

            TargetCameraLocalY    = _settings.NormalCameraLocalY;
            CharController.height = _settings.NormalHeight;
            CharController.center = new Vector3(0f, _settings.NormalHeight * 0.5f, 0f);

            StateMachine.Initialize(IdleState);
        }

        private void OnEnable()
        {
            _input.JumpStarted  += OnJumpStarted;
            _input.SlideStarted += OnSlideStarted;
            _input.SlideEnded   += OnSlideEnded;
        }

        private void OnDisable()
        {
            _input.JumpStarted  -= OnJumpStarted;
            _input.SlideStarted -= OnSlideStarted;
            _input.SlideEnded   -= OnSlideEnded;
        }

        private void Update()
        {
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
        }

        private void FixedUpdate() => StateMachine.FixedTick();

        // ── Public helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Smoothly resizes the CharacterController capsule toward targetHeight.
        /// Centre is kept at half-height so the base stays at the character's feet.
        /// </summary>
        public void SetControllerHeight(float targetHeight)
        {
            float next = Mathf.Lerp(CharController.height, targetHeight,
                                    _settings.HeightSmoothSpeed * Time.deltaTime);
            CharController.height = next;
            CharController.center = new Vector3(0f, next * 0.5f, 0f);
        }

        // ── Input callbacks ──────────────────────────────────────────────────────
        private void OnJumpStarted()  => JumpRequested = true;
        private void OnSlideStarted() => SlideHeld     = true;
        private void OnSlideEnded()   => SlideHeld     = false;
    }
}
