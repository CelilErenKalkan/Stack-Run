using Game_Management;
using System.Collections.Generic;
using UnityEngine;

public class StackSpawner : MonoBehaviour
{
    private GameObject currentGrid;
    private GameObject previousGrid;
    private int currentZ = 0;

    public static List<GridData> gridHistory = new List<GridData>();

    private void OnEnable()
    {
        Actions.SetNextGrid += OnNextGrid;
    }

    private void OnDisable()
    {
        Actions.SetNextGrid -= OnNextGrid;
    }

    private void Start()
    {
        SpawnInitialGrid();
    }

    private void SpawnInitialGrid()
    {
        previousGrid = Pool.Instance.SpawnObject(Vector3.zero, PoolItemType.Grid, null);
        RecordGrid(previousGrid);
        SpawnNextGrid();
    }

    private void OnNextGrid(GameObject lastPlacedGrid)
    {
        previousGrid = lastPlacedGrid;
        RecordGrid(lastPlacedGrid);
        SpawnNextGrid();
    }

    private void SpawnNextGrid()
    {
        currentZ++;
        Vector3 newPosition = new Vector3(0, 0, currentZ * 1f); // assuming 1f spacing in Z

        currentGrid = Pool.Instance.SpawnObject(newPosition, PoolItemType.Grid, null);
        if (currentGrid.TryGetComponent<GridController>(out GridController controller))
        {
            controller.Init();
        }

        // Reset scale
        currentGrid.transform.localScale = new Vector3(1f, 0.2f, 1f);
    }

    private void RecordGrid(GameObject grid)
    {
        Vector3 pos = grid.transform.position;
        float scaleX = grid.transform.localScale.x;
        gridHistory.Add(new GridData(pos.x, pos.y, scaleX));
    }
}

[System.Serializable]
public struct GridData
{
    public float x;
    public float y;
    public float scaleX;

    public GridData(float x, float y, float scaleX)
    {
        this.x = x;
        this.y = y;
        this.scaleX = scaleX;
    }
}
