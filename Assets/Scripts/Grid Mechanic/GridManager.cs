using System.Collections.Generic;
using Data_Management;
using Game_Management;
using UnityEngine;

namespace Grid_Mechanic
{
    public static class GridManager
    {
        private static float zStackInterval = 1.0f;
        private static float perfectMatchTolerance = 0.01f;
        private static int gridCount = 0;

        private static List<GridData> currentLevel = new List<GridData>();

        public static bool IsInputLocked { get; set; } = false;

        private static GridMovementController _activeGridMovementController;

        private static bool isReachedFinishLine(float zPos)
        {
            return GameManager.Instance.FinishLine != null && (GameManager.Instance.FinishLine.position.z - zPos <= 0.5f);
        }

        #region Spawning

        public static void UpdateLevelEnd()
        {
            DataManager.SaveOnLevelEnd(currentLevel);
            Actions.ResetAllGrids?.Invoke();
            GameManager.Instance.FollowTarget.transform.position = Vector3.zero;
            currentLevel.Clear();
            SpawnPreviousLevel();
        }

        public static void SpawnPreviousLevel()
        {
            var previousLevel = DataManager.previousLevel;
            Debug.Log("sp" + previousLevel.Count);
            var zDifference = previousLevel.Count - 1;
        
            for (var i = 0; i < previousLevel.Count; i++)
            {
                SpawnGrid(i - zDifference, previousLevel[i].materialIndex);
            }

            GameManager.Instance.FinishLine.position = new Vector3(0f, 0f, 0.5f);
        }

        private static GameObject SpawnGrid(float z)
        {
            Vector3 spawnPos = new Vector3(0f, 0f, z);

            return Pool.Instance.SpawnObject(spawnPos, PoolItemType.Grid, null);
        }
    
        private static GameObject SpawnGrid(float z, int materialIndex)
        {
            Vector3 spawnPos = new Vector3(0f, 0f, z);
        
            var grid = Pool.Instance.SpawnObject(spawnPos, PoolItemType.Grid, null);
        
            if (grid.TryGetComponent(out GridVisualController vController))
                vController.SetMaterialByIndex(materialIndex);

            return grid;
        }

        public static void SpawnInitialGrid()
        {
            var initialGrid = SpawnGrid(0.0f);

            if (initialGrid != null 
                && initialGrid.TryGetComponent(out GridMovementController mController)
                && initialGrid.TryGetComponent(out GridVisualController vController))
            {
                mController.Init(false);
                gridCount = 1;

                var gridData = new GridData(0.0f, 1f, vController.AssignedMaterialIndex);
                currentLevel.Add(gridData); // ✅ Add to current level only
            }

            SpawnNextGrid();
        }


        public static void SpawnNextGrid()
        {
            float nextZ = gridCount * zStackInterval;

            if (isReachedFinishLine(nextZ))
            {
                //Actions.LevelFinished?.Invoke();
            }
            else
            {
                var nextGrid = SpawnGrid(nextZ);
            
                if (nextGrid != null && nextGrid.TryGetComponent(out GridMovementController controller))
                {
                    controller.Init(true);
                    _activeGridMovementController = controller;
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

                if (part.TryGetComponent<GridVisualController>(out var vController))
                {
                    vController.SetMaterialByIndex(materialIndex);
                }
            }
        }

        public static void SpawnFinishLine()
        {
            int multiplier = DataManager.GetLevel / 10;
            float distanceZ = (multiplier * 10 + DataManager.GetLevel + 5.5f) % 100;
            Vector3 finishLinePos = new Vector3(0, 0.1f, distanceZ);
            GameManager.Instance.SetFinishLine(Pool.Instance.SpawnObject(finishLinePos, PoolItemType.Finish, null).transform);
        }

        #endregion

        #region Click & Evaluation

        public static void HandleClickInput()
        {
            if (!Input.GetMouseButtonDown(0) || IsInputLocked || _activeGridMovementController == null)
                return;

            IsInputLocked = true;
            _activeGridMovementController.StopMovement();
            if (_activeGridMovementController.TryGetComponent(out GridVisualController vController))
                EvaluateAndProcessActiveGrid(_activeGridMovementController, vController);
        }

        private static void EvaluateAndProcessActiveGrid(GridMovementController gridMovement, GridVisualController gridVisual)
        {
            if (currentLevel.Count == 0)
            {
                Debug.LogWarning("No previous grid data found.");
                return;
            }

            GridData previous = currentLevel[^1];

            if (!TryEvaluateOverlap(previous, gridMovement.transform, gridMovement.MatchThreshold, out float overlapWidth, out float overlapLeft, out float overlapRight))
            {
                Debug.Log("Game Over! Grid missed completely.");
                gridMovement.ApplyGravity();
                return;
            }

            float currLeft = gridMovement.transform.position.x - gridMovement.transform.localScale.x / 2f;
            float currRight = gridMovement.transform.position.x + gridMovement.transform.localScale.x / 2f;

            if (IsPerfectMatch(gridMovement.transform.localScale.x, overlapWidth))
            {
                gridMovement.SnapTo(previous.x, previous.scaleX);
                gridVisual.AnimateEmission(true);
                DataManager.SetScore(2);
            }
            else
            {
                gridMovement.TrimAndSpawnFallingParts(currLeft, currRight, overlapLeft, overlapRight, overlapWidth);
                DataManager.SetScore(1);
            }

            // Add to currentLevel list instead of DataManager
            currentLevel.Add(new GridData(
                gridMovement.transform.position.x,
                gridMovement.transform.localScale.x,
                gridVisual.AssignedMaterialIndex
            ));

            Actions.SetNextGrid?.Invoke(gridMovement.gameObject);
            GameManager.Instance.UpdateFollowTarget(gridMovement.transform.position, previous.x);
        }

        private static bool TryEvaluateOverlap(GridData previous, Transform currentTransform, float matchThreshold, out float overlapWidth, out float overlapLeft, out float overlapRight)
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

        private static bool IsPerfectMatch(float originalWidth, float overlapWidth)
        {
            return Mathf.Abs(originalWidth - overlapWidth) < perfectMatchTolerance;
        }

        #endregion
    }
}
