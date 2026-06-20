using UnityEngine;

namespace FPS.Controller
{
    /// <summary>
    /// Player is grounded with no movement input.
    ///
    /// Friction bleeds off any residual horizontal velocity every frame using
    /// the same ApplyFriction call as RunState so deceleration feels consistent.
    ///
    /// Transitions:
    ///   → RunState  when WASD input is detected
    ///   → JumpState when jump is pressed
    /// </summary>
    public class FPSIdleState : FPSBaseState
    {
        public FPSIdleState(FPSMovementController controller) : base(controller) { }

        public override void Enter()
        {
            Controller.TargetCameraLocalY = Controller.Settings.NormalCameraLocalY;
        }

        public override void Tick()
        {
            Controller.SetControllerHeight(Controller.Settings.NormalHeight);

            if (Controller.JumpRequested && Controller.IsGrounded)
            {
                Controller.StateMachine.TransitionTo(Controller.JumpState);
                return;
            }

            if (Controller.MoveInput.sqrMagnitude > 0.01f)
            {
                Controller.StateMachine.TransitionTo(Controller.RunState);
                return;
            }

            // Bleed residual velocity with ground friction; Accelerate is not
            // called here because there is no wishdir to accelerate toward.
            ApplyFriction(Controller.Settings.Friction);
        }

        public override void Exit() { }
    }
}
