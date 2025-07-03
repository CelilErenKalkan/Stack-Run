using DG.Tweening;
using UnityEngine;

namespace Grid_Mechanic
{
    [RequireComponent(typeof(GridVisualController))]
    public class GridMovementController : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float gridHeight = 0.2f;
        [SerializeField] private float moveRange = 5f;
        [SerializeField] private float moveDuration = 3f;
        [SerializeField] private float matchThreshold = 0.3f;

        private Tween moveTween;
        private Transform cachedTransform;

        public float MatchThreshold => matchThreshold;
        public float GridHeight => gridHeight;

        public GridVisualController VisualController { get; private set; }

        private void Awake()
        {
            cachedTransform = transform;
            VisualController = GetComponent<GridVisualController>();
        }

        public void Init(bool shouldMove = true)
        {
            GridManager.IsInputLocked = false;
            cachedTransform.localScale = new Vector3(1f, gridHeight, 1f);
            VisualController.AssignRandomMaterial();

            if (shouldMove)
                StartMovement();
        }

        private void StartMovement()
        {
            moveTween = cachedTransform.DOMoveX(moveRange, moveDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .From(-moveRange);
        }

        public void StopMovement()
        {
            moveTween?.Kill();
        }

        public void ApplyGravity()
        {
            if (TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }

        public void SnapTo(float prevX, float prevScaleX)
        {
            cachedTransform.localScale = new Vector3(prevScaleX, gridHeight, 1f);
            cachedTransform.position = new Vector3(prevX, cachedTransform.position.y, cachedTransform.position.z);
        }

        public void TrimAndSpawnFallingParts(float currLeft, float currRight, float overlapLeft, float overlapRight, float overlapWidth)
        {
            cachedTransform.localScale = new Vector3(overlapWidth, gridHeight, 1f);
            cachedTransform.position = new Vector3((overlapLeft + overlapRight) / 2f, cachedTransform.position.y, cachedTransform.position.z);

            float leftTrim = overlapLeft - currLeft;
            if (leftTrim > 0)
            {
                Vector3 leftFallPos = new(currLeft + leftTrim / 2f, cachedTransform.position.y, cachedTransform.position.z);
                GridManager.SpawnFallingPart(leftFallPos, leftTrim, VisualController.AssignedMaterialIndex);
            }

            float rightTrim = currRight - overlapRight;
            if (rightTrim > 0)
            {
                Vector3 rightFallPos = new(currRight - rightTrim / 2f, cachedTransform.position.y, cachedTransform.position.z);
                GridManager.SpawnFallingPart(rightFallPos, rightTrim, VisualController.AssignedMaterialIndex);
            }
        }

        public Transform GetTransform() => cachedTransform;
        public float GetScaleX() => cachedTransform.localScale.x;
        public float GetPositionX() => cachedTransform.position.x;
        public Vector3 GetPosition() => cachedTransform.position;
    }
}
