using System.Collections.Generic;
using Data_Management;
using Game_Management;
using UnityEngine;

namespace Grid_Mechanic
{
    public static class GridManager
    {
        private static float zStackInterval = 1.0f;
        private static float perfectMatchTolerance = 0.1f;
        private static int gridCount = 0;

        private static List<GridData> currentLevel = new List<GridData>();
        public static List<GridData> CurrentLevel => currentLevel;

        public static bool IsInputLocked { get; set; } = false;

        private static GridMovementController _activeGridMovementController;

        // Material cycling
        private static int materialCounter = 0;

        private static bool isReachedFinishLine(float zPos)
        {
            return GameManager.Instance.FinishLine != null && (GameManager.Instance.FinishLine.position.z - zPos <= 0.5f);
        }

        #region Spawning

        public static void UpdateLevelEnd(bool isSuccess)
        {
            if (isSuccess)
                DataManager.SaveOnLevelEnd(currentLevel);
            Actions.ResetAllGrids?.Invoke();
            SpawnPreviousLevel();
            ResetFinishLine();
            currentLevel.Clear();
        }

        public static void SpawnPreviousLevel()
        {
            var previousLevel = DataManager.previousLevel;
            var zDifference = previousLevel.Count;

            for (var i = 0; i < previousLevel.Count; i++)
            {
                SpawnGrid(i - zDifference, previousLevel[i].materialIndex, previousLevel[i].scaleX);
            }
        }

        private static GameObject SpawnGrid(float z, float scaleX = 1)
        {
            Vector3 spawnPos = new Vector3(0f, 0f, z);
            var grid = Pool.Instance.SpawnObject(spawnPos, PoolItemType.Grid, null);
            grid.transform.localScale = new Vector3(scaleX, 0.2f, 1f);

            return grid;
        }

        private static GameObject SpawnGrid(float z, int materialIndex, float scaleX = 1)
        {
            Vector3 spawnPos = new Vector3(0f, 0f, z);

            var grid = Pool.Instance.SpawnObject(spawnPos, PoolItemType.Grid, null);
            grid.transform.localScale = new Vector3(scaleX, 0.2f, 1f);

            if (grid.TryGetComponent(out GridVisualController vController))
                vController.SetMaterialByIndex(materialIndex);

            return grid;
        }

        private static void SetGridMaterial(GridVisualController vController)
        {
            if (materialCounter <= 0)
                materialCounter = Random.Range(0, 100);
            
            vController.SetMaterialByIndex(materialCounter);
            materialCounter++;
        }

        public static void SpawnInitialGrid()
        {
            var initialGrid = SpawnGrid(1.0f);

            if (initialGrid != null &&
                initialGrid.TryGetComponent(out GridMovementController mController) &&
                initialGrid.TryGetComponent(out GridVisualController vController))
            {
                mController.Init(false); // fixed duration for static grid
                gridCount = 1;

                SetGridMaterial(vController);
                var gridData = new GridData(0.0f, 1f, materialCounter);
                currentLevel.Add(gridData);
            }

            Actions.SetNextGrid?.Invoke(initialGrid);
            GameManager.Instance.UpdateFollowTarget(initialGrid.transform.position, 0f);
        }

        public static void SpawnNextGrid()
        {
            float nextZ = gridCount * zStackInterval + 1;

            if (!isReachedFinishLine(nextZ))
            {
                float prevScaleX = 1f;
                if (currentLevel.Count > 0)
                    prevScaleX = currentLevel[^1].scaleX;

                var nextGrid = SpawnGrid(nextZ, prevScaleX);
                if (nextGrid == null) return;

                if (nextGrid.TryGetComponent(out GridMovementController controller))
                {
                    controller.Init(true);
                    _activeGridMovementController = controller;
                    gridCount++;
                }

                if (nextGrid.TryGetComponent(out GridVisualController vController))
                {
                    SetGridMaterial(vController);
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

        public static float FinishLineDistanceCalculation()
        {
            var multiplier = DataManager.GetLevel / 10;
            return (multiplier * 10 + DataManager.GetLevel + 10.0f) % 100;
        }

        public static void SpawnFinishLine(float distanceZ)
        {
            Vector3 finishLinePos = new Vector3(0, 0.1f, distanceZ);
            GameManager.Instance.SetFinishLine(Pool.Instance.SpawnObject(finishLinePos, PoolItemType.Finish, null).transform);
        }

        private static void ResetFinishLine()
        {
            if (GameManager.Instance.FinishLine != null)
            {
                Pool.Instance.DeactivateObject(GameManager.Instance.FinishLine.gameObject, PoolItemType.Finish);
                GameManager.Instance.SetFinishLine(null);
            }
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
                gridMovement.ApplyGravity();
                return;
            }

            float currLeft = gridMovement.transform.position.x - gridMovement.transform.localScale.x / 2f;
            float currRight = gridMovement.transform.position.x + gridMovement.transform.localScale.x / 2f;

            if (IsPerfectMatch(gridMovement.transform.localScale.x, overlapWidth))
            {
                Actions.PerfectNote?.Invoke();
                gridMovement.SnapTo(previous.x, previous.scaleX);
                gridVisual.AnimateEmission(true);
                DataManager.SetScore(2);
            }
            else
            {
                Actions.StandardNote?.Invoke();
                gridMovement.TrimAndSpawnFallingParts(currLeft, currRight, overlapLeft, overlapRight, overlapWidth);
                DataManager.SetScore(1);
            }

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
