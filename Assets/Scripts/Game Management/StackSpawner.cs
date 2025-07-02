using Game_Management;
using UnityEngine;

public class StackSpawner : MonoBehaviour
{
    private void OnEnable()
    {
        Actions.SetNextGrid += OnNextGridRequested;
        Actions.LevelStarted += OnLevelStarted;
    }

    private void OnDisable()
    {
        Actions.SetNextGrid -= OnNextGridRequested;
        Actions.LevelStarted -= OnLevelStarted;
    }

    private void Start()
    {
        
    }

    private void OnLevelStarted()
    {
        GridManager.SetFinishLine();
        GridManager.SpawnInitialGrid();
    }

    private void OnNextGridRequested(GameObject previousGrid)
    {
        Debug.Log("Next grid requested, spawning...");
        GridManager.SpawnNextGrid();
    }
}

