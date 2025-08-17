using System;
using UnityEngine;

namespace Prototype.Scripts.GenricScripts
{
    public class TopDownCameraController : MonoBehaviour
    {
        [Header("Movement")]
        public float Speed = 5f;

        [Header("Zoom")]
        public float ZoomInterpolationSpeed = 0.3f;
        public float ZoomTarget = 3f;
        public float ZoomIncrement = 2f;
        private float _refVelocity;
        
        [Header("Camera")]
        [SerializeField]
        private Transform _cameraTarget;
        [SerializeField]
        private Camera _camera;
        [SerializeField] 
        private GameObject _cameraArm;

        // == Section : Unity Event == \\
        private void Awake()
        {
            _camera ??= Camera.main;
            _cameraTarget = transform;
        }

        private void Update()
        {
            HandleMovement();
            HandleZoom();
        }

        // == Section : Movement == \\
        private void HandleMovement()
        {
            var dir = GetMovementDirection();
            transform.Translate(dir * (Speed * Time.deltaTime), Space.World);
        }
        
        private Vector3 GetMovementDirection()
        {
            var movementDirection = Vector3.zero;

            if (Input.GetKey(KeyCode.W))
            {
                movementDirection += Vector3.forward;
            }

            if (Input.GetKey(KeyCode.A))
            {
                movementDirection += Vector3.left;
            }

            if (Input.GetKey(KeyCode.S))
            {
                movementDirection += Vector3.back;
            }

            if (Input.GetKey(KeyCode.D))
            {
                movementDirection += Vector3.right;
            }
        
            return movementDirection.normalized;
        }
        
        // == Section : Zoom == \\
        private void HandleZoom()
        {
            // exit if no Camera Arm exists
            if (!_cameraArm)
                return;
            
            var zoomOffset = Input.GetAxis("Mouse ScrollWheel") * ZoomIncrement;
            ZoomTarget += zoomOffset;
            ZoomTarget = Mathf.Clamp(ZoomTarget, 1f, 20f);
            
            _cameraArm.transform.localPosition = new Vector3(
                _cameraArm.transform.localPosition.x, 
                Mathf.SmoothDamp(_cameraArm.transform.localPosition.y, ZoomTarget,ref _refVelocity, ZoomInterpolationSpeed),
                _cameraArm.transform.localPosition.z);
            _camera.transform.LookAt(_cameraTarget);
        }
    }
}
