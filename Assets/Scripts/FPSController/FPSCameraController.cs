using UnityEngine;

namespace FPS.Controller
{
    /// <summary>
    /// Handles FPS mouse-look and slide camera-dip.
    ///
    /// Place this component on the Camera GameObject that is a child of the
    /// player body.  Yaw (left/right) is applied to the player body so that
    /// CharacterController movement always faces the correct direction.
    /// Pitch (up/down) is applied only to this camera's local rotation.
    /// Camera height is smoothly driven toward TargetCameraLocalY which the
    /// active movement state updates.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class FPSCameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FPSInputReader       _input;
        [SerializeField] private FPSPlayerSettings    _settings;
        [SerializeField] private FPSMovementController _movementController;

        private float _pitch;
        private float _yaw;

        private void Start()
        {
            // Seed yaw from the player body's current heading so the camera
            // starts aligned and doesn't snap on the first frame.
            _yaw   = _movementController.transform.eulerAngles.y;
            _pitch = transform.localEulerAngles.x;

            // Normalise pitch into [-180, 180] to avoid Clamp wrapping issues
            if (_pitch > 180f) _pitch -= 360f;
        }

        private void LateUpdate()
        {
            Vector2 delta = _input.LookInput * _settings.MouseSensitivity;

            _yaw   += delta.x;
            _pitch -= delta.y;  // subtract: positive mouse-Y should look up
            _pitch  = Mathf.Clamp(_pitch, -_settings.MaxPitchAngle, _settings.MaxPitchAngle);

            // Horizontal rotation is owned by the player body so movement
            // always matches the look direction.
            _movementController.transform.rotation = Quaternion.Euler(0f, _yaw, 0f);

            // Vertical rotation is local to the camera only.
            transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

            // Smooth camera height for crouch / slide dip.
            Vector3 localPos = transform.localPosition;
            localPos.y = Mathf.Lerp(localPos.y,
                                    _movementController.TargetCameraLocalY,
                                    _settings.CameraHeightSmooth * Time.deltaTime);
            transform.localPosition = localPos;
        }
    }
}
