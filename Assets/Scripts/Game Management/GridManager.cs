using Data_Management;
using Game_Management;
using UnityEngine;

public static class GridManager
{
    private static float zStackInterval = 1.0f;
    private static float perfectMatchTolerance = 0.01f;

    private static int gridCount = 0;

    public static bool IsInputLocked { get; set; } = false;


    #region Spawn Functions
    public static void SpawnInitialGrid()
    {
        Vector3 initialPosition = Vector3.zero;
        GameObject initialGrid = Pool.Instance.SpawnObject(initialPosition, PoolItemType.Grid, null);

        if (initialGrid != null && initialGrid.TryGetComponent<GridController>(out GridController controller))
        {
            controller.Init(false); // Initial grid does NOT move
            gridCount = 1;

            DataManager.previousLevel.Add(new GridData(initialPosition.x, initialPosition.y, 1f, controller.AssignedMaterialIndex));
        }

        SpawnNextGrid();  // Start the stacking process immediately
    }

    public static void SpawnNextGrid()
    {
        float nextZ = gridCount * zStackInterval;

        if (GameManager.Instance.isReachedFinishLine(nextZ))
        {
            Actions.LevelFinished?.Invoke();
        }
        else
        {
            Vector3 spawnPos = new Vector3(0f, 0f, nextZ);

            GameObject nextGrid = Pool.Instance.SpawnObject(spawnPos, PoolItemType.Grid, null);
            if (nextGrid != null && nextGrid.TryGetComponent<GridController>(out GridController controller))
            {
                controller.Init(true); // Moving grid
                gridCount++;
            }
            else
            {
                Debug.LogWarning("Failed to spawn or initialize next grid.");
            }
        }
    }

    public static void SpawnFallingPart(Vector3 position, float width, int materialIndex)
    {
        GameObject part = Pool.Instance.SpawnObject(position, PoolItemType.Grid, null, 3f);
        if (part != null)
        {
            part.transform.localScale = new Vector3(width, 0.2f, 1f);

            if (part.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }

            if (part.TryGetComponent<GridController>(out var controller))
            {
                controller.SetMaterialByIndex(materialIndex);
            }
        }
    }

    public static void SetFinishLine()
    {
        int multiplier = DataManager.GetLevel / 10;
        float distanceZ = (multiplier * 10 + DataManager.GetLevel + +5.5f) % 100;
        Vector3 finishLinePos = new Vector3(0, 0.1f, distanceZ);
        GameManager.Instance.SetFinishLine(Pool.Instance.SpawnObject(finishLinePos, PoolItemType.Finish, null).transform);
    }

    #endregion

    #region Evaluator

    public static bool TryEvaluateOverlap(GridData previous, Transform currentTransform, float matchThreshold, out float overlapWidth, out float overlapLeft, out float overlapRight)
    {
        float prevLeft = previous.x - previous.scaleX / 2f;
        float prevRight = previous.x + previous.scaleX / 2f;

        float currX = currentTransform.position.x;
        float currScaleX = currentTransform.localScale.x;
        float currLeft = currX - currScaleX / 2f;
        float currRight = currX + currScaleX / 2f;

        overlapLeft = Mathf.Max(prevLeft, currLeft);
        overlapRight = Mathf.Min(prevRight, currRight);
        overlapWidth = overlapRight - overlapLeft;

        return overlapWidth > matchThreshold;
    }

    public static bool IsPerfectMatch(float originalWidth, float overlapWidth)
    {
        return Mathf.Abs(originalWidth - overlapWidth) < perfectMatchTolerance;
    }

    #endregion
}
