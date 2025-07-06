using DG.Tweening;
using UnityEngine;

namespace Grid_Mechanic
{
    [RequireComponent(typeof(GridVisualController))]
    public class GridMovementController : MonoBehaviour
    {
        [Header("Grid Settings")]
        [SerializeField] private float moveRange = 2f;
        [SerializeField] private float moveDuration = 0.2f;
        [SerializeField] private float matchThreshold = 0.3f;

        private Tween moveTween;
        private Transform cachedTransform;
        private bool moveFromLeft = true;

        public float MatchThreshold => matchThreshold;
        public GridVisualController VisualController { get; private set; }

        private void Awake()
        {
            cachedTransform = transform;
            VisualController = GetComponent<GridVisualController>();
        }

        /// <summary>
        /// Standard init with random direction.
        /// </summary>
        public void Init(bool shouldMove = true)
        {
            Init(shouldMove, Random.value > 0.5f);
        }

        /// <summary>
        /// Init with explicit direction.
        /// </summary>
        public void Init(bool shouldMove, bool moveFromLeftSide)
        {
            GridManager.IsInputLocked = false;
            VisualController.AssignRandomMaterial();
            moveFromLeft = moveFromLeftSide;

            if (shouldMove)
                StartMovement();
        }

        private void StartMovement()
        {
            float fromX = moveFromLeft ? -moveRange : moveRange;
            float targetX = moveRange * (moveFromLeft ? 1 : -1);

            moveTween = cachedTransform.DOMoveX(targetX, moveDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .From(fromX);
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
            cachedTransform.localScale = new Vector3(prevScaleX, 0.2f, 1f);
            cachedTransform.position = new Vector3(prevX, cachedTransform.position.y, cachedTransform.position.z);
        }

        public void TrimAndSpawnFallingParts(float currLeft, float currRight, float overlapLeft, float overlapRight, float overlapWidth)
        {
            cachedTransform.localScale = new Vector3(overlapWidth, 0.2f, 1f);
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
    }
}
