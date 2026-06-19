using UnityEngine;

namespace FPS.Controller
{
    /// <summary>
    /// Quake / Titanfall-2-style movement parameters.
    /// All speed values are in Unity units per second.
    /// Variable names intentionally mirror idTech / Source engine conventions.
    /// </summary>
    [CreateAssetMenu(fileName = "FPSPlayerSettings", menuName = "FPS/Player Settings")]
    public class FPSPlayerSettings : ScriptableObject
    {
        [Header("Ground Movement")]
        [Tooltip("Maximum ground speed.")]
        public float MoveSpeed          = 7f;

        [Tooltip("Rate at which ground speed builds toward MoveSpeed.")]
        public float GroundAccelerate   = 14f;

        [Tooltip("Rate at which ground speed bleeds off when there is no input (applied on top of friction).")]
        public float GroundDecelerate   = 10f;

        [Tooltip("Quake sv_friction — multiplied against current speed to compute the per-frame drag drop.")]
        public float Friction           = 6f;

        [Tooltip("Quake sv_stopspeed — when speed is below this value friction is applied at maximum " +
                 "strength, preventing the player from drifting forever at low speeds.")]
        public float StopSpeed          = 1.5f;

        [Header("Air Movement")]
        [Tooltip("Acceleration rate while airborne (intentionally low; this is what makes strafe-jumping possible).")]
        public float AirAccelerate      = 2f;

        [Tooltip("Hard cap on speed added per AirAccelerate call. " +
                 "Quake uses ~30 ups (~1.2 Unity units). " +
                 "Lower = tighter bhop window; higher = easier speed build.")]
        public float AirSpeedCap        = 1.2f;

        [Tooltip("CPMA-style perpendicular side-strafe acceleration for sharp directional air control.")]
        public float SideStrafeAccel    = 50f;

        [Tooltip("Maximum speed added per frame by a pure side-strafe input. " +
                 "Keep this small (Quake CPMA ~ 0.35 Unity units) to preserve inertia.")]
        public float SideStrafeSpeed    = 0.35f;

        [Header("Jump")]
        [Tooltip("Vertical speed applied as an impulse on jump (Unity units / s).")]
        public float JumpSpeed          = 8f;

        [Tooltip("Downward gravity magnitude (positive value, applied as -Y each frame).")]
        public float Gravity            = 20f;

        [Header("Slide (Titanfall 2 style)")]
        [Tooltip("Minimum horizontal speed required to begin a slide.")]
        public float MinSlideEntrySpeed = 2f;

        [Tooltip("Flat speed bonus injected the moment a slide starts — the signature Titanfall 2 momentum pop.")]
        public float SlideSpeedBoost    = 4f;

        [Tooltip("Surface drag applied during a slide (lower than Friction so speed is carried).")]
        public float SlideFriction      = 1.5f;

        [Tooltip("Maximum slide duration before the state expires.")]
        public float MaxSlideDuration   = 1.2f;

        [Header("Controller Capsule")]
        public float NormalHeight       = 2f;
        public float SlideHeight        = 1f;
        public float HeightSmoothSpeed  = 15f;

        [Header("Camera")]
        public float MouseSensitivity   = 0.15f;
        public float MaxPitchAngle      = 80f;
        public float NormalCameraLocalY = 1.6f;
        public float SlideCameraLocalY  = 0.5f;
        public float CameraHeightSmooth = 12f;
    }
}
