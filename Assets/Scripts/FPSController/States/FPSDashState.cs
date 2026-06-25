using UnityEngine;

namespace FPS.Controller
{
    /// <summary>
    /// Player dashes regardless of in air or on ground.
    ///
    /// Adds Impulse and then let friction decrease the speed.
    ///
    /// 
    /// Transitions:
    ///   -> When Dash finishes, (timer or distance), it exits to idle state.
    /// </summary>
    ///
    public class FPSDashState : FPSBaseState
    {
        public FPSDashState(FPSMovementController controller) : base(controller) { }

        private float dashDuration;
        
        
        public override void Enter()
        {
            Vector3 desiredDirection = BuildWishDir();

            // Use whatever ur facing towards.
            if (desiredDirection == Vector3.zero)
            {
                desiredDirection = Controller.transform.forward;
            }
            
            Controller.Velocity = desiredDirection * Controller.Settings.dashStrength; //dashSpeed.

            dashDuration = Controller.Settings.dashDuration;
        }

        public override void Tick()
        {
            dashDuration -= Time.deltaTime;
            ApplyFriction(Controller.Settings.Friction);
            
            if (dashDuration <= 0f)
            {
                Controller.StateMachine.TransitionTo(Controller.IdleState);
            }
        }

        public override void Exit() { }
    }
}