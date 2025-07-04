using Game_Management;
using UnityEngine;

namespace Camera
{
    public class CameraRotation : MonoBehaviour
    {
        [HideInInspector] public float rotationSpeed = 500f; // Slower rotation speed
        [SerializeField] private Transform _followTarget;

        // Update is called once per frame
        private void LateUpdate()
        {
            if (_followTarget)
                RotateAroundTarget();
        }
        
        private void OnEnable()
        {
            
            Actions.LevelStarted += OnLevelStarted;
            Actions.LevelFinished += OnLevelFinished;
        }

        private void OnDisable()
        {
            Actions.LevelStarted -= OnLevelStarted;
            Actions.LevelFinished -= OnLevelFinished;
        }
        
        private void OnLevelStarted()
        {
            SetCameraTarget(null);
        }

        private void OnLevelFinished()
        {
            SetCameraTarget(GameManager.Instance.Chibi.transform);
        }

        private void SetCameraTarget(Transform target)
        {
            _followTarget = target;
        }
    
        private void RotateAroundTarget()
        {
            // Rotate the camera around the target
            transform.RotateAround(_followTarget.position, Vector3.down, rotationSpeed * Time.deltaTime);
            transform.LookAt(_followTarget);
        }
    }
}
