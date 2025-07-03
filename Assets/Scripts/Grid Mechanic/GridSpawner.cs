using System;
using Data_Management;
using Game_Management;
using UnityEngine;

namespace Grid_Mechanic
{
    public class GridSpawner : MonoBehaviour
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
            if (DataManager.previousLevel.Count > 0)
                GridManager.SpawnPreviousLevel();
        }

        private void OnLevelStarted()
        {
            GridManager.SpawnFinishLine();
            GridManager.SpawnInitialGrid();
        }

        private void OnNextGridRequested(GameObject previousGrid)
        {
            Debug.Log("Next grid requested, spawning...");
            GridManager.SpawnNextGrid();
        }
    }
}

