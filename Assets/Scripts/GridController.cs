using DG.Tweening;
using Game_Management;
using UnityEngine;

public class GridController : MonoBehaviour
{
    private float matchThreshold = 0.3f;
    private float moveRange = 10f;
    private float duration = 1.0f;
    private Tween moveTween;

    public void Init()
    {
        transform.localScale = new Vector3(1f, 1f, 1f);
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
        if (Input.GetMouseButtonDown(0))
        {
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
            // Perfect or near-perfect match
            Vector3 newPos = transform.position;
            newPos.x = prevX;
            transform.position = newPos;
        }
        else if (absDeltaX < prevScaleX / 2f)
        {
            // Partial match
            float newScaleX = prevScaleX - absDeltaX;
            float direction = Mathf.Sign(deltaX);

            // Resize current grid
            Vector3 scale = transform.localScale;
            scale.x = newScaleX;
            transform.localScale = scale;

            Vector3 newPos = transform.position;
            newPos.x = prevX + deltaX / 2f;
            transform.position = newPos;

            // Spawn falling part
            float fallScaleX = absDeltaX;
            Vector3 fallPos = transform.position;
            fallPos.x = prevX + direction * (newScaleX / 2f + fallScaleX / 2f);

            GameObject fallingPart = Pool.Instance.SpawnObject(fallPos, PoolItemType.Grid, null, 3f);
            if (fallingPart != null)
            {
                fallingPart.transform.localScale = new Vector3(fallScaleX, 1f, 1f);
            }
        }
        else
        {
            // Missed - game over condition
            Debug.Log("Game Over! Grid missed completely.");
            return;
        }

        // Call SetNextGrid event with this grid
        Actions.SetNextGrid?.Invoke(this.gameObject);
    }
}
