using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data_Management
{
    [System.Serializable]
    public struct GridData
    {
        public float x;
        public float scaleX;
        public int materialIndex;

        public GridData(float x, float scaleX, int materialIndex)
        {
            this.x = x;
            this.scaleX = scaleX;
            this.materialIndex = materialIndex;
        }
    }

    [Serializable] // Required for JSON serialization
    public struct GameData
    {
        // Public fields or properties to be serialized
        
        public int score;
        public int levelNo;
        public bool isMuted;

        // Constructor with default values
        public GameData(int score = 0, int levelNo = 1, bool isMuted = false)
        {
            this.score = score;
            this.levelNo = levelNo;
            this.isMuted = isMuted;
        }
    }


    public static class DataManager
    {
        public static GameData gameData;
        public static List<GridData> previousLevel;

        public static int GetLevel => gameData.levelNo;
        public static int GetScore => gameData.score;

        public static void SaveOnLevelEnd(List<GridData> currentLevel)
        {
            previousLevel = currentLevel;
            Debug.Log("pL" + previousLevel.Count);
            SaveLevelOrder();
            gameData.levelNo++;
            SaveData();
        }

        public static void SetScore(int amount)
        {
            gameData.score += amount;
        }

        #region Data Management

        /// <summary>
        /// Loads all the data from the files with error handling.
        /// </summary>
        public static void LoadData()
        {
            try
            {
                gameData = FileHandler.ReadFromJson<GameData>("PlayerData.json");
                SaveData();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data: {ex.Message}");
                gameData = new GameData();
                SaveData();
            }
        }

        /// <summary>
        /// Saves all the data to the files with error handling.
        /// </summary>
        public static void SaveData()
        {
            try
            {
                FileHandler.SaveToJson(gameData, "GameData.json");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save data: {ex.Message}");
            }
        }

        #endregion

        #region Level Management

        private static void SaveLevelOrder()
        {
            try
            {
                FileHandler.SaveListToJson(previousLevel, "PreviousLevel.json");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save previous level: {ex.Message}");
            }
        }

        public static void LoadLevelOrder()
        {
            try
            {
                previousLevel = FileHandler.ReadListFromJson<GridData>("PreviousLevel.json");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load previous level: {ex.Message}");
                previousLevel = new List<GridData>(); // Initialize empty list in case of failure.
            }
        }

        #endregion
    }
}
