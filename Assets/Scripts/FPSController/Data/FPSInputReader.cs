using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FPS.Controller
{
    /// <summary>
    /// ScriptableObject input bridge.  Binds to the existing InputActionAsset
    /// (Assets/InputSystem_Actions.inputactions → Player map) and exposes
    /// cooked values and events consumed by the controller and states.
    ///
    /// Assign the InputSystem_Actions asset in the Inspector field.
    /// </summary>
    [CreateAssetMenu(fileName = "FPSInputReader", menuName = "FPS/Input Reader")]
    public class FPSInputReader : ScriptableObject
    {
        [SerializeField] private InputActionAsset _actionAsset;

        // ── Polled values ────────────────────────────────────────────────────────
        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }

        // ── One-shot events ──────────────────────────────────────────────────────
        public event Action JumpStarted;
        public event Action SlideStarted;
        public event Action SlideEnded;

        // ── Private action references ────────────────────────────────────────────
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _slideAction;

        private void OnEnable()
        {
            if (_actionAsset == null)
            {
                Debug.LogError("[FPSInputReader] InputActionAsset is not assigned.");
                return;
            }

            var map    = _actionAsset.FindActionMap("Player", throwIfNotFound: true);
            _moveAction  = map.FindAction("Move",   throwIfNotFound: true);
            _lookAction  = map.FindAction("Look",   throwIfNotFound: true);
            _jumpAction  = map.FindAction("Jump",   throwIfNotFound: true);
            _slideAction = map.FindAction("Crouch", throwIfNotFound: true);

            _moveAction.performed  += HandleMove;
            _moveAction.canceled   += HandleMove;
            _lookAction.performed  += HandleLook;
            _lookAction.canceled   += HandleLook;
            _jumpAction.started    += HandleJump;
            _slideAction.started   += HandleSlideStarted;
            _slideAction.canceled  += HandleSlideEnded;

            map.Enable();
        }

        private void OnDisable()
        {
            if (_moveAction == null) return;

            _moveAction.performed  -= HandleMove;
            _moveAction.canceled   -= HandleMove;
            _lookAction.performed  -= HandleLook;
            _lookAction.canceled   -= HandleLook;
            _jumpAction.started    -= HandleJump;
            _slideAction.started   -= HandleSlideStarted;
            _slideAction.canceled  -= HandleSlideEnded;

            _actionAsset?.FindActionMap("Player")?.Disable();

            MoveInput = Vector2.zero;
            LookInput = Vector2.zero;
        }

        // ── Handlers ─────────────────────────────────────────────────────────────
        private void HandleMove(InputAction.CallbackContext ctx)
            => MoveInput = ctx.ReadValue<Vector2>();

        private void HandleLook(InputAction.CallbackContext ctx)
            => LookInput = ctx.ReadValue<Vector2>();

        private void HandleJump(InputAction.CallbackContext ctx)
            => JumpStarted?.Invoke();

        private void HandleSlideStarted(InputAction.CallbackContext ctx)
            => SlideStarted?.Invoke();

        private void HandleSlideEnded(InputAction.CallbackContext ctx)
            => SlideEnded?.Invoke();
    }
}
