using UnityEngine;

namespace FPS.Controller
{
    /// <summary>
    /// Player is airborne.
    ///
    /// Air movement uses AirAccelerate — a direct port of Quake's PM_AirAccelerate:
    ///
    ///   cappedWishspeed = min(wishspeed, AirSpeedCap)       // hard limit per call
    ///   currentspeed    = dot(velocity, wishdir)
    ///   addspeed        = cappedWishspeed - currentspeed
    ///   accelspeed      = AirAccelerate * wishspeed * dt    // clamped to addspeed
    ///   velocity       += accelspeed * wishdir
    ///
    /// Because AirSpeedCap is small (≈ 1.2 units) the player can only add a tiny
    /// amount of speed each frame, but when wishdir is perpendicular to velocity
    /// the dot product is zero so addspeed always equals AirSpeedCap.  By
    /// continuously steering perpendicular to their trajectory (strafe-jumping)
    /// the player accumulates speed beyond MoveSpeed indefinitely.
    ///
    /// Pure side-strafe inputs additionally receive a CPMA acceleration spike
    /// (SideStrafeAccel / SideStrafeSpeed) for snappy directional air control
    /// that matches the Titanfall 2 feel.
    ///
    /// Horizontal momentum from the previous state is fully preserved on Enter()
    /// so slide-jump and run-jump carry their speed into the air.
    ///
    /// Transitions:
    ///   → RunState  when the player lands and has movement input
    ///   → IdleState when the player lands with no input
    /// </summary>
    public class FPSJumpState : FPSBaseState
    {
        public FPSJumpState(FPSMovementController controller) : base(controller) { }

        public override void Enter()
        {
            // Fire vertical impulse; preserve all horizontal momentum so run/slide
            // speed carries cleanly into the air (Titanfall 2 behaviour).
            Controller.Velocity = new Vector3(
                Controller.Velocity.x,
                Controller.Settings.JumpSpeed,
                Controller.Velocity.z);

            Controller.TargetCameraLocalY = Controller.Settings.NormalCameraLocalY;
        }

        public override void Tick()
        {
            Controller.SetControllerHeight(Controller.Settings.NormalHeight);

            Vector3 wishdir    = BuildWishDir();
            float   wishspeed  = Controller.Settings.MoveSpeed;

            // AirAccelerate handles both the standard Quake path and the CPMA
            // side-strafe spike depending on the shape of the input vector.
            if (wishdir.sqrMagnitude > 0.001f)
                AirAccelerate(wishdir, wishspeed);

            // Land when descending and touching ground.  The Velocity.y <= 0 guard
            // prevents a false landing trigger on the frame the jump fires, where
            // IsGrounded can still be true for one physics step.
            if (Controller.IsGrounded && Controller.Velocity.y <= 0f)
            {
                bool hasInput = Controller.MoveInput.sqrMagnitude > 0.01f;
                Controller.StateMachine.TransitionTo(hasInput
                    ? (IState)Controller.RunState
                    : Controller.IdleState);
            }
            if (Controller.DashRequested)
            {
                Controller.StateMachine.TransitionTo(Controller.DashState);
                return;
            }
        }

        public override void Exit() { }
    }
}
