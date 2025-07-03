using Data_Management;
using DG.Tweening;
using Game_Management;
using System.Collections.Generic;
using UnityEngine;

public class GridController : MonoBehaviour
{
    //Visuals
    [SerializeField] private Renderer gridRenderer;
    [SerializeField] private List<Material> materialOptions;

    //Movement Settings
    private float moveRange = 5f;
    private float moveDuration = 3f;

    //Grid Settings
    private float gridHeight = 0.2f;
    [SerializeField] private float matchThreshold = 0.3f;

    //Emission Settings
    private float emissionIntensity = 2f;
    private float emissionDuration = 0.5f;

    private Tween moveTween;
    private Tween colorTween;

    private bool isActive = false;

    private Transform cachedTransform;

    public int AssignedMaterialIndex { get; private set; } = -1;

    private void Awake()
    {
        cachedTransform = transform;
    }

    private void Update()
    {
        if (!isActive || GridManager.IsInputLocked) return;

        if (Input.GetMouseButtonUp(0))
        {
            GridManager.IsInputLocked = true;
            isActive = false;

            StopMovement();
            HandleClick();
        }
    }

    #region Initialization

    public void Init(bool shouldMove = true)
    {
        isActive = shouldMove;
        GridManager.IsInputLocked = false;

        cachedTransform.localScale = new Vector3(1f, gridHeight, 1f);

        AssignRandomMaterial();

        if (shouldMove)
            StartMovement();
    }

    private void AssignRandomMaterial()
    {
        if (materialOptions == null || materialOptions.Count == 0)
        {
            Debug.LogWarning("Material options list is empty!");
            return;
        }

        AssignedMaterialIndex = Random.Range(0, materialOptions.Count);
        gridRenderer.material = materialOptions[AssignedMaterialIndex];
    }

    #endregion

    #region Movement

    private void StartMovement()
    {
        moveTween = cachedTransform.DOMoveX(moveRange, moveDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.Linear)
            .From(-moveRange);
    }

    private void StopMovement()
    {
        moveTween?.Kill();
    }

    #endregion

    #region Interaction

    private void HandleClick()
    {
        if (DataManager.previousLevel.Count == 0)
        {
            Debug.LogWarning("No previous grid data found.");
            return;
        }

        var previous = DataManager.previousLevel[^1];
        float prevX = previous.x;
        float prevScaleX = previous.scaleX;

        float currX = cachedTransform.position.x;
        float currScaleX = cachedTransform.localScale.x;

        float prevLeft = prevX - prevScaleX / 2f;
        float prevRight = prevX + prevScaleX / 2f;
        float currLeft = currX - currScaleX / 2f;
        float currRight = currX + currScaleX / 2f;

        float overlapLeft = Mathf.Max(prevLeft, currLeft);
        float overlapRight = Mathf.Min(prevRight, currRight);
        float overlapWidth = overlapRight - overlapLeft;

        if (overlapWidth <= 0 || overlapWidth < matchThreshold)
        {
            Debug.Log("Game Over! Grid missed completely.");
            ApplyGravity();
            return;
        }

        if (GridManager.IsPerfectMatch(currScaleX, overlapWidth))
        {
            SnapToPrevious(prevX, prevScaleX);
            AnimateEmission(true);
            DataManager.SetScore(2);
        }
        else
        {
            TrimAndFallParts(currLeft, currRight, overlapLeft, overlapRight, overlapWidth);
            DataManager.SetScore(1);
        }

        RecordGridData();
        Actions.SetNextGrid?.Invoke(this.gameObject);
        GameManager.Instance.UpdateFollowTarget(cachedTransform.position, prevX);
    }

    private void ApplyGravity()
    {
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void SnapToPrevious(float prevX, float prevScaleX)
    {
        cachedTransform.localScale = new Vector3(prevScaleX, gridHeight, 1f);
        cachedTransform.position = new Vector3(prevX, cachedTransform.position.y, cachedTransform.position.z);
    }

    private void TrimAndFallParts(float currLeft, float currRight, float overlapLeft, float overlapRight, float overlapWidth)
    {
        cachedTransform.localScale = new Vector3(overlapWidth, gridHeight, 1f);
        cachedTransform.position = new Vector3((overlapLeft + overlapRight) / 2f, cachedTransform.position.y, cachedTransform.position.z);

        float leftTrim = overlapLeft - currLeft;
        if (leftTrim > 0)
        {
            Vector3 leftFallPos = new(currLeft + leftTrim / 2f, cachedTransform.position.y, cachedTransform.position.z);
            GridManager.SpawnFallingPart(leftFallPos, leftTrim, AssignedMaterialIndex);
        }

        float rightTrim = currRight - overlapRight;
        if (rightTrim > 0)
        {
            Vector3 rightFallPos = new(currRight - rightTrim / 2f, cachedTransform.position.y, cachedTransform.position.z);
            GridManager.SpawnFallingPart(rightFallPos, rightTrim, AssignedMaterialIndex);
        }
    }

    private void RecordGridData()
    {
        DataManager.AddNewGrid(new GridData(
            cachedTransform.position.x,
            cachedTransform.position.y,
            cachedTransform.localScale.x,
            AssignedMaterialIndex
        ));
    }

    #endregion

    #region Visuals

    public void SetMaterialByIndex(int index)
    {
        if (materialOptions == null || index < 0 || index >= materialOptions.Count)
            return;

        AssignedMaterialIndex = index;
        Material matInstance = Instantiate(materialOptions[AssignedMaterialIndex]);
        gridRenderer.material = matInstance;
    }

    private void AnimateEmission(bool glow)
    {
        if (gridRenderer == null || !gridRenderer.material.HasProperty("_EmissionColor"))
            return;

        Color targetColor = glow ? Color.white * emissionIntensity : Color.black;

        colorTween?.Kill();
        colorTween = DOTween.To(
            () => gridRenderer.material.GetColor("_EmissionColor"),
            c => gridRenderer.material.SetColor("_EmissionColor", c),
            targetColor,
            emissionDuration
        ).SetLoops(glow ? 2 : 1, LoopType.Yoyo);
    }

    #endregion
}
