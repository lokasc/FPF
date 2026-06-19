using UnityEngine;

namespace FPS.Controller
{
    /// <summary>
    /// Player is grounded and actively moving.
    ///
    /// Ground movement uses the Quake Accelerate function:
    ///   currentspeed = dot(velocity, wishdir)
    ///   addspeed     = wishspeed - currentspeed
    ///   accelspeed   = GroundAccelerate * wishspeed * dt  (clamped to addspeed)
    ///   velocity    += accelspeed * wishdir
    ///
    /// Friction is applied first so the player changes direction cleanly.
    ///
    /// Transitions:
    ///   → IdleState  when all movement input is released
    ///   → JumpState  when jump is pressed
    ///   → SlideState when crouch is held at sufficient speed
    /// </summary>
    public class FPSRunState : FPSBaseState
    {
        public FPSRunState(FPSMovementController controller) : base(controller) { }

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

            if (Controller.SlideHeld && Controller.IsGrounded)
            {
                float hSpeed = new Vector3(Controller.Velocity.x, 0f, Controller.Velocity.z).magnitude;
                if (hSpeed >= Controller.Settings.MinSlideEntrySpeed)
                {
                    Controller.StateMachine.TransitionTo(Controller.SlideState);
                    return;
                }
            }

            if (Controller.Input.MoveInput.sqrMagnitude <= 0.01f)
            {
                Controller.StateMachine.TransitionTo(Controller.IdleState);
                return;
            }

            // Quake ground physics: friction first, then accelerate.
            // Applying friction before Accelerate means a 180° direction change
            // feels crisp — friction removes backward speed, Accelerate builds
            // forward speed, all within a single frame.
            ApplyFriction(Controller.Settings.Friction);

            Vector3 wishdir = BuildWishDir();
            Accelerate(wishdir, Controller.Settings.MoveSpeed, Controller.Settings.GroundAccelerate);
        }

        public override void Exit() { }
    }
}
