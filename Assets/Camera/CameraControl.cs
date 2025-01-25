using UnityEngine.InputSystem;
using UnityEngine;

namespace CH.Character.Player
{
    public class CameraControl : MonoBehaviour
    {
        private InputActionMap _InputActionMap;
        private bool _SpinningMouse = false;

        public InputActionAsset Controls;
        public GameObject Character;
        public float CameraSmoothing = 0.15f;

        [Header("Spin")]
        public float SpinFactor = 10.0f;

        private void OnMousePerform(InputAction.CallbackContext context)
        {
            _SpinningMouse = true;

            Debug.Log("Moving Camera");
        }

        private void OnMouseRelease(InputAction.CallbackContext context)
        {
            _SpinningMouse = false;
            Debug.Log("Mouse Release, Camera");
        }

        private Vector3 GetPlayerDir()
        {
            var dir = Character.transform.forward;
            return dir;
        }

        private void SpinCameraControl()
        {
            var currentMouse = Mouse.current.position;

            Vector2 dir2D = currentMouse.value;
            Vector3 dir = new(0, dir2D.x, 0);
            float magnitude = currentMouse.magnitude * SpinFactor;

            Quaternion newQuat = Quaternion.AngleAxis(magnitude, dir);
            var lerpQuat = Quaternion.Lerp(transform.rotation, newQuat, CameraSmoothing);
            transform.rotation = lerpQuat;
        }

        private void CameraStandardControl()
        {
            Vector3 dir = GetPlayerDir();
            var newQuat = Quaternion.LookRotation(dir);

            var lerpQuat = Quaternion.Lerp(transform.rotation, newQuat, CameraSmoothing);

            transform.rotation = lerpQuat;
        }

        public void Update()
        {
            if (!_SpinningMouse)
                CameraStandardControl();
            else
                SpinCameraControl();
        }

        public void Start()
        {
            _InputActionMap = Controls.FindActionMap("gameplay");

            var MoveCamera = _InputActionMap.FindAction("move-camera");
            MoveCamera.performed += OnMousePerform;
            MoveCamera.canceled += OnMouseRelease;
        }
    }
}