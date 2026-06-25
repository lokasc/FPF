using UnityEngine;

namespace FPS.Controller
{
    /// <summary>
    /// Abstract base for all FPS states.
    ///
    /// Contains faithful ports of the three core idTech / Quake movement primitives:
    ///
    ///   Accelerate      — ground movement (Quake PM_Accelerate)
    ///   AirAccelerate   — air movement with optional side-strafe spike (Quake PM_AirAccelerate + CPMA air-control)
    ///   ApplyFriction   — surface drag (Quake PM_Friction)
    ///
    /// These functions operate only on the horizontal plane.  Vertical velocity
    /// (gravity, jump impulse) is owned exclusively by FPSMovementController.
    ///
    /// How strafe-jumping works (the dot-product trick):
    ///   currentspeed = dot(velocity, wishdir)
    ///   addspeed     = min(wishspeed, AirSpeedCap) - currentspeed
    ///
    /// When wishdir is perpendicular to velocity the dot product is 0, so
    /// addspeed equals the full AirSpeedCap every frame, letting the player
    /// accumulate speed beyond MoveSpeed purely through directional input.
    /// </summary>
    public abstract class FPSBaseState : IState
    {
        protected readonly FPSMovementController Controller;

        protected FPSBaseState(FPSMovementController controller)
        {
            Controller = controller;
        }

        public abstract void Enter();
        public abstract void Tick();
        public virtual  void FixedTick() { }
        public abstract void Exit();

        // ── Movement Primitives ──────────────────────────────────────────────────

        /// <summary>
        /// Quake PM_Accelerate — ground acceleration toward wishdir at wishspeed.
        /// Respects the dot-product cap so the player cannot exceed MoveSpeed on
        /// the ground, but deceleration from the previous direction is instant.
        /// </summary>
        /// <param name="wishdir">Normalised desired movement direction (horizontal only).</param>
        /// <param name="wishspeed">Target speed (usually MoveSpeed).</param>
        /// <param name="accel">Acceleration constant (GroundAccelerate).</param>
        protected void Accelerate(Vector3 wishdir, float wishspeed, float accel)
        {
            Vector3 horizontal   = new Vector3(Controller.Velocity.x, 0f, Controller.Velocity.z);
            float   currentspeed = Vector3.Dot(horizontal, wishdir);
            float   addspeed     = wishspeed - currentspeed;

            if (addspeed <= 0f) return;

            float accelspeed = accel * wishspeed * Time.deltaTime;
            if (accelspeed > addspeed) accelspeed = addspeed;

            horizontal += accelspeed * wishdir;
            Controller.Velocity = new Vector3(horizontal.x, Controller.Velocity.y, horizontal.z);
        }

        /// <summary>
        /// Quake PM_AirAccelerate — the function that makes bhop / strafe-jumping
        /// possible.  The wishspeed used for dot-product comparison is hard-capped
        /// at AirSpeedCap (≈ 1.2 units), so the player can always add a tiny
        /// sliver of speed perpendicular to their current trajectory.
        ///
        /// Additionally, pure side-strafe inputs (|input.x| > |input.y|) receive
        /// a brief CPMA-style acceleration spike (SideStrafeAccel) for snappier
        /// directional air control, matching the Titanfall 2 feel.
        /// </summary>
        /// <param name="wishdir">Normalised desired movement direction (horizontal only).</param>
        /// <param name="wishspeed">Uncapped target speed.</param>
        protected void AirAccelerate(Vector3 wishdir, float wishspeed)
        {
            FPSPlayerSettings s = Controller.Settings;

            Vector2 input   = Controller.MoveInput;
            bool isSideStrafe = Mathf.Abs(input.x) > Mathf.Abs(input.y) && Mathf.Abs(input.y) < 0.1f;

            float accel;
            float cappedWishspeed;

            if (isSideStrafe)
            {
                // CPMA perpendicular air-control spike: full side-strafe speed build
                accel            = s.SideStrafeAccel;
                cappedWishspeed  = s.SideStrafeSpeed;
            }
            else
            {
                // Standard Quake air acceleration with AirSpeedCap
                accel            = s.AirAccelerate;
                cappedWishspeed  = Mathf.Min(wishspeed, s.AirSpeedCap);
            }

            Vector3 horizontal   = new Vector3(Controller.Velocity.x, 0f, Controller.Velocity.z);
            float   currentspeed = Vector3.Dot(horizontal, wishdir);
            float   addspeed     = cappedWishspeed - currentspeed;

            if (addspeed <= 0f) return;

            float accelspeed = accel * wishspeed * Time.deltaTime;
            if (accelspeed > addspeed) accelspeed = addspeed;

            horizontal += accelspeed * wishdir;
            Controller.Velocity = new Vector3(horizontal.x, Controller.Velocity.y, horizontal.z);
        }

        /// <summary>
        /// Quake PM_Friction — applies surface drag to horizontal velocity.
        /// Uses StopSpeed as a minimum speed denominator so the player does not
        /// drift forever at very low speeds.
        /// </summary>
        /// <param name="frictionCoeff">Surface friction (Friction for ground, SlideFriction for slides).</param>
        protected void ApplyFriction(float frictionCoeff)
        {
            FPSPlayerSettings s = Controller.Settings;

            Vector3 horizontal = new Vector3(Controller.Velocity.x, 0f, Controller.Velocity.z);
            float   speed      = horizontal.magnitude;

            if (speed < 0.01f)
            {
                Controller.Velocity = new Vector3(0f, Controller.Velocity.y, 0f);
                return;
            }

            // Use StopSpeed as a floor so low-speed drag is not negligible
            float control    = speed < s.StopSpeed ? s.StopSpeed : speed;
            float drop       = control * frictionCoeff * Time.deltaTime;
            float newspeed   = Mathf.Max(speed - drop, 0f);

            horizontal      *= newspeed / speed;
            Controller.Velocity = new Vector3(horizontal.x, Controller.Velocity.y, horizontal.z);
        }

        // ── Shared helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Lucas' TLDR - WISHDIR is getting wasd in vector3 form and returning it to u.
        /// 
        /// Builds the normalised wish direction from WASD input relative to the
        /// player body's forward/right axes (horizontal plane only).
        /// Returns Vector3.zero when there is no input.
        /// </summary>
        protected Vector3 BuildWishDir()
        {
            Vector2   input  = Controller.MoveInput;
            Transform body   = Controller.transform;
            Vector3   dir    = body.right * input.x + body.forward * input.y;
            dir.y            = 0f;
            return dir.sqrMagnitude > 0.001f ? dir.normalized : Vector3.zero;
        }
    }
}
