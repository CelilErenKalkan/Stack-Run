using Game_Management;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StackSpawner : MonoBehaviour
{
    public static List<GridData> gridHistory = new List<GridData>();

    [SerializeField] private float zStackInterval = 1.0f;

    private int gridCount = 0;

    private void OnEnable()
    {
        Actions.SetNextGrid += OnNextGridRequested;
    }

    private void OnDisable()
    {
        Actions.SetNextGrid -= OnNextGridRequested;
    }

    private void Start()
    {
        SetFinishLine();
        SpawnInitialGrid();
    }

    private void SpawnInitialGrid()
    {
        Vector3 initialPosition = Vector3.zero;
        GameObject initialGrid = Pool.Instance.SpawnObject(initialPosition, PoolItemType.Grid, null);

        if (initialGrid != null && initialGrid.TryGetComponent<GridController>(out GridController controller))
        {
            controller.Init(false); // Initial grid does NOT move
            gridCount = 1;

            gridHistory.Add(new GridData(initialPosition.x, initialPosition.y, 1f, controller.assignedMaterialIndex));
        }

        SpawnNextGrid();  // Start the stacking process immediately
    }

    private void SetFinishLine()
    {
        int multiplier = GameManager.Instance.GetLevel() / 10;
        float distanceZ = (multiplier * 10 + GameManager.Instance.GetLevel() +  + 5.5f) % 100;
        Vector3 finishLinePos = new Vector3(0, 0.1f, distanceZ);
        GameManager.Instance.finishLine = Pool.Instance.SpawnObject(finishLinePos, PoolItemType.Finish, null).transform;
    }

    private void OnNextGridRequested(GameObject previousGrid)
    {
        Debug.Log("Next grid requested, spawning...");
        SpawnNextGrid();
    }

    private void SpawnNextGrid()
    {
        float nextZ = gridCount * zStackInterval;

        if (GameManager.Instance.CheckFinisLine(nextZ))
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
}




[System.Serializable]
public struct GridData
{
    public float x;
    public float y;
    public float scaleX;
    public int materialIndex;

    public GridData(float x, float y, float scaleX, int materialIndex)
    {
        this.x = x;
        this.y = y;
        this.scaleX = scaleX;
        this.materialIndex = materialIndex;
    }
}

