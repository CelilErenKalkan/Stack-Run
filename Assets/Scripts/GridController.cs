using DG.Tweening;
using Game_Management;
using UnityEngine;

public class GridController : MonoBehaviour
{
    private float matchThreshold = 0.3f;
    private float moveRange = 5.0f;
    private float duration = 3.0f;
    private Tween moveTween;

    private bool isActive = false;
    public static bool isInputLocked = false;

    public void Init()
    {
        isActive = true;
        isInputLocked = false;

        transform.localScale = new Vector3(1f, 0.2f, 1f); // ✅ Set Y to 0.2f
        StartMoving();
    }

    private void StartMoving()
    {
        moveTween = transform.DOMoveX(moveRange, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear)
            .From(-moveRange);
    }

    private void Update()
    {
        if (!isActive || isInputLocked)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            isInputLocked = true;
            isActive = false;

            StopMovement();
            HandleClick();
        }
    }

    private void StopMovement()
    {
        moveTween.Kill();
    }

    private void HandleClick()
    {
        if (StackSpawner.gridHistory.Count == 0)
        {
            Debug.LogWarning("No previous grid data found.");
            return;
        }

        GridData previous = StackSpawner.gridHistory[^1]; // last item
        float prevX = previous.x;
        float prevScaleX = previous.scaleX;

        float currX = transform.position.x;
        float deltaX = currX - prevX;
        float absDeltaX = Mathf.Abs(deltaX);

        if (absDeltaX <= matchThreshold)
        {
            // ✅ Perfect match: snap position and copy scale
            Vector3 newPos = transform.position;
            newPos.x = prevX;
            transform.position = newPos;

            transform.localScale = new Vector3(prevScaleX, 0.2f, 1f); // ✅ Set Y to 0.2f
        }
        else if (absDeltaX < prevScaleX / 2f)
        {
            // ✅ Partial match: trim current grid and spawn falling part
            float newScaleX = prevScaleX - absDeltaX;
            float direction = Mathf.Sign(deltaX);

            Vector3 scale = transform.localScale;
            scale.x = newScaleX;
            scale.y = 0.2f; // ✅ Enforce Y scale
            transform.localScale = scale;

            Vector3 newPos = transform.position;
            newPos.x = prevX + deltaX / 2f;
            transform.position = newPos;

            float fallScaleX = absDeltaX;
            Vector3 fallPos = transform.position;
            fallPos.x = prevX + direction * (newScaleX / 2f + fallScaleX / 2f);

            GameObject fallingPart = Pool.Instance.SpawnObject(fallPos, PoolItemType.Grid, null, 3f);
            if (fallingPart != null)
            {
                fallingPart.transform.localScale = new Vector3(fallScaleX, 0.2f, 1f); // ✅ Set Y to 0.2f

                if (fallingPart.TryGetComponent<Rigidbody>(out Rigidbody fallRb))
                {
                    fallRb.isKinematic = false;
                    fallRb.useGravity = true;
                }
            }
        }
        else
        {
            // ❌ Missed completely — let the current grid fall
            Debug.Log("Game Over! Grid missed completely.");

            if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            return;
        }

        // ✅ Continue to next grid
        Actions.SetNextGrid?.Invoke(this.gameObject);
    }
}
