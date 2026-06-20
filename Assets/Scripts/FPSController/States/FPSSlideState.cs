using UnityEngine;

namespace FPS.Controller
{
    /// <summary>
    /// Titanfall 2-style ground slide.
    ///
    /// On Enter:
    ///   - Direction is locked to the body's forward at the moment of entry.
    ///   - A flat SlideSpeedBoost is added to the entry speed, giving the
    ///     signature Titanfall 2 momentum "pop".
    ///   - The capsule shrinks to SlideHeight for clearance under obstacles.
    ///
    /// During Tick:
    ///   - SlideFriction (lower than ground Friction) slowly bleeds off speed,
    ///     letting the slide carry significant momentum.
    ///   - No player steering is applied — the slide travels in the locked direction.
    ///
    /// Jump-out-of-slide:
    ///   - Full horizontal slide velocity is preserved into JumpState, producing
    ///     the "slide-jump" speed boost that defines Titanfall 2 movement.
    ///
    /// Transitions:
    ///   → JumpState when jump is pressed (preserves full slide momentum)
    ///   → RunState  when slide expires / button released + movement input
    ///   → IdleState when slide expires / button released + no input
    /// </summary>
    public class FPSSlideState : FPSBaseState
    {
        private float   _slideTimer;
        private Vector3 _slideDirection;

        public FPSSlideState(FPSMovementController controller) : base(controller) { }

        public override void Enter()
        {
            _slideTimer     = 0f;
            _slideDirection = Controller.transform.forward;

            // Titanfall 2 momentum pop: boost entry speed and lock it to the slide direction
            float   currentHSpeed = new Vector3(Controller.Velocity.x, 0f, Controller.Velocity.z).magnitude;
            float   entrySpeed    = currentHSpeed + Controller.Settings.SlideSpeedBoost;

            Controller.Velocity = new Vector3(
                _slideDirection.x * entrySpeed,
                Controller.Velocity.y,
                _slideDirection.z * entrySpeed);

            Controller.TargetCameraLocalY = Controller.Settings.SlideCameraLocalY;
        }

        public override void Tick()
        {
            _slideTimer += Time.deltaTime;

            Controller.SetControllerHeight(Controller.Settings.SlideHeight);

            // Jump-out preserves the full current slide velocity — this is the
            // Titanfall 2 slide-jump that lets players chain speed between surfaces.
            if (Controller.JumpRequested && Controller.IsGrounded)
            {
                Controller.StateMachine.TransitionTo(Controller.JumpState);
                return;
            }

            bool expired  = _slideTimer >= Controller.Settings.MaxSlideDuration;
            bool released = !Controller.SlideHeld;

            if (expired || released)
            {
                bool hasInput = Controller.MoveInput.sqrMagnitude > 0.01f;
                Controller.StateMachine.TransitionTo(hasInput
                    ? (IState)Controller.RunState
                    : Controller.IdleState);
                return;
            }

            // Apply reduced slide friction in the locked direction only.
            // We re-project onto _slideDirection after friction so the velocity
            // vector never drifts sideways due to floating-point error.
            ApplyFriction(Controller.Settings.SlideFriction);

            Vector3 horizontal = new Vector3(Controller.Velocity.x, 0f, Controller.Velocity.z);
            float   speed      = horizontal.magnitude;
            Controller.Velocity = new Vector3(
                _slideDirection.x * speed,
                Controller.Velocity.y,
                _slideDirection.z * speed);
        }

        public override void Exit()
        {
            Controller.TargetCameraLocalY = Controller.Settings.NormalCameraLocalY;
        }
    }
}
