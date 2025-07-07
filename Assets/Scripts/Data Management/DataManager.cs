using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data_Management
{
    [Serializable]
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

    [Serializable] // Enables JSON serialization
    public struct GameData
    {
        public int levelNo;
        public bool isMuted;

        public GameData(int levelNo = 1, bool isMuted = false)
        {
            this.levelNo = levelNo;
            this.isMuted = isMuted;
        }
    }

    public static class DataManager
    {
        public static GameData gameData;
        public static List<GridData> previousLevel;

        public static int GetLevel => gameData.levelNo;

        /// <summary>
        /// Saves current level data, increments level number, and writes to disk.
        /// </summary>
        public static void SaveOnLevelEnd(List<GridData> currentLevel)
        {
            previousLevel = currentLevel;
            SaveLevelOrder();
            gameData.levelNo++;
            SaveData();
        }

        /// <summary>
        /// Loads player progression and audio preferences from disk.
        /// </summary>
        public static void LoadData()
        {
            try
            {
                gameData = FileHandler.ReadFromJson<GameData>("GameData.json");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load data: {ex.Message}");
                gameData = new GameData(); // Fall back to defaults
                SaveData(); // Save the default state
            }
        }

        /// <summary>
        /// Saves player progression and audio preferences to disk.
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

        /// <summary>
        /// Saves the level structure (grid layout) to disk.
        /// </summary>
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

        /// <summary>
        /// Loads the last level's structure from disk.
        /// </summary>
        public static void LoadLevelOrder()
        {
            try
            {
                previousLevel = FileHandler.ReadListFromJson<GridData>("PreviousLevel.json");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load previous level: {ex.Message}");
                previousLevel = new List<GridData>(); // Fallback to prevent null reference
            }
        }
    }
}
