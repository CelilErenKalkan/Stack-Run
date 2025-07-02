using DG.Tweening;
using Game_Management;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    [SerializeField] private Renderer gridRenderer;
    [SerializeField] private List<Material> materialOptions;

    private float matchThreshold = 0.3f;
    private float moveRange = 5.0f;
    private float duration = 3.0f;
    private Tween moveTween;
    private Tween colorTween;

    private bool isActive = false;
    public static bool isInputLocked = false;

    public int assignedMaterialIndex = -1;

    public void Init(bool shouldMove = true)
    {
        isActive = shouldMove;
        isInputLocked = false;

        transform.localScale = new Vector3(1f, 0.2f, 1f);

        AssignRandomMaterial();

        if (shouldMove)
            StartMoving();
    }

    private void AssignRandomMaterial()
    {
        if (materialOptions != null && materialOptions.Count > 0)
        {
            assignedMaterialIndex = Random.Range(0, materialOptions.Count);
            Material matInstance = Instantiate(materialOptions[assignedMaterialIndex]);
            gridRenderer.material = matInstance;
        }
        else
        {
            Debug.LogWarning("Material options list is empty!");
        }
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
        moveTween?.Kill();
    }

    private void HandleClick()
    {
        if (GridManager.gridHistory.Count == 0)
        {
            Debug.LogWarning("No previous grid data found.");
            return;
        }

        GridData previous = GridManager.gridHistory[^1];
        float prevX = previous.x;
        float prevScaleX = previous.scaleX;

        float currX = transform.position.x;
        float currScaleX = transform.localScale.x;

        // Calculate edges of previous and current grids
        float prevLeft = prevX - prevScaleX / 2f;
        float prevRight = prevX + prevScaleX / 2f;

        float currLeft = currX - currScaleX / 2f;
        float currRight = currX + currScaleX / 2f;

        // Calculate overlap edges
        float overlapLeft = Mathf.Max(prevLeft, currLeft);
        float overlapRight = Mathf.Min(prevRight, currRight);
        float overlapWidth = overlapRight - overlapLeft;

        if (overlapWidth <= 0 || overlapWidth < matchThreshold)
        {
            // No or very small overlap → game over: let grid fall
            Debug.Log("Game Over! Grid missed completely.");

            if (TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            return;
        }

        // Perfect overlap (almost equal scale and position)
        if (Mathf.Abs(currScaleX - overlapWidth) < 0.01f)
        {
            // Snap to previous scale and position
            transform.localScale = new Vector3(prevScaleX, 0.2f, 1f);
            Vector3 newPos = transform.position;
            newPos.x = prevX;
            transform.position = newPos;

            AnimateEmission(true);
            GameManager.Instance.AddScore(2);
        }
        else
        {
            // Partial overlap - trim current grid to overlap area
            transform.localScale = new Vector3(overlapWidth, 0.2f, 1f);
            Vector3 newPos = transform.position;
            newPos.x = (overlapLeft + overlapRight) / 2f;
            transform.position = newPos;

            // Spawn falling parts (left side)
            float leftTrimWidth = overlapLeft - currLeft;
            if (leftTrimWidth > 0)
            {
                Vector3 fallPos = new Vector3(currLeft + leftTrimWidth / 2f, transform.position.y, transform.position.z);
                GridManager.SpawnFallingPart(fallPos, leftTrimWidth, assignedMaterialIndex);
            }

            // Spawn falling parts (right side)
            float rightTrimWidth = currRight - overlapRight;
            if (rightTrimWidth > 0)
            {
                Vector3 fallPos = new Vector3(currRight - rightTrimWidth / 2f, transform.position.y, transform.position.z);
                GridManager.SpawnFallingPart(fallPos, rightTrimWidth, assignedMaterialIndex);
            }

            GameManager.Instance.AddScore(1);
        }

        // Record current grid data
        GridManager.gridHistory.Add(new GridData(transform.position.x, transform.position.y, transform.localScale.x, assignedMaterialIndex));

        Actions.SetNextGrid?.Invoke(this.gameObject);
        GameManager.Instance.UpdateFollowTarget(transform.position, GridManager.gridHistory[^1].x);
    }

    public void SetMaterialByIndex(int index)
    {
        if (materialOptions != null && index >= 0 && index < materialOptions.Count)
        {
            assignedMaterialIndex = index;
            Material matInstance = Instantiate(materialOptions[assignedMaterialIndex]);
            gridRenderer.material = matInstance;
        }
    }

    private void AnimateEmission(bool glow)
    {
        if (gridRenderer == null)
            return;

        Material mat = gridRenderer.material;

        if (!mat.HasProperty("_EmissionColor"))
            return;

        Color targetColor = glow ? Color.white * 2f : Color.black;
        if (colorTween != null && colorTween.IsActive()) colorTween.Kill();

        colorTween = DOTween.To(
            () => mat.GetColor("_EmissionColor"),
            c => mat.SetColor("_EmissionColor", c),
            targetColor,
            0.5f
        ).SetLoops(glow ? 2 : 1, LoopType.Yoyo);
    }
}
