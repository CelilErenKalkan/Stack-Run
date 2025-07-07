using Game_Management;
using UnityEngine;

namespace Camera
{
    public class CameraRotation : MonoBehaviour
    {
        [HideInInspector] public float rotationSpeed = 500f; // Controls how fast the camera rotates
        [SerializeField] private Transform _followTarget;

        private void OnEnable()
        {
            // Subscribe to level events
            Actions.LevelStarted += OnLevelStarted;
            Actions.LevelFinished += OnLevelFinished;
        }

        private void OnDisable()
        {
            // Unsubscribe from events to avoid memory leaks
            Actions.LevelStarted -= OnLevelStarted;
            Actions.LevelFinished -= OnLevelFinished;
        }

        private void LateUpdate()
        {
            // Rotate the camera only if a target is set
            if (_followTarget != null)
                RotateAroundTarget();
        }

        private void OnLevelStarted()
        {
            // Stop rotating when level starts (target is cleared)
            SetCameraTarget(null);
        }

        private void OnLevelFinished()
        {
            // Start rotating around the chibi character when level ends
            SetCameraTarget(GameManager.Instance.Chibi.transform);
        }

        private void SetCameraTarget(Transform target)
        {
            _followTarget = target;
        }

        private void RotateAroundTarget()
        {
            // Smoothly rotate the camera around the target on the Y-axis (Vector3.down = clockwise)
            transform.RotateAround(_followTarget.position, Vector3.down, rotationSpeed * Time.deltaTime);

            // Ensure the camera is always looking at the target
            transform.LookAt(_followTarget);
        }
    }
}