using System.Collections.Generic;
using Cinemachine;
using Game_Management;
using UnityEngine;

namespace Camera
{
    public class CameraManager : MonoBehaviour
    {
        // A list of virtual cameras to switch between.
        [SerializeField] private List<CinemachineVirtualCamera> cameraList;

        private void OnEnable()
        {
            // Subscribe to game events
            Actions.LevelStarted += OnLevelStarted;
            Actions.LevelFinished += OnLevelFinished;
        }

        private void OnDisable()
        {
            // Unsubscribe to prevent memory leaks or null reference issues
            Actions.LevelStarted -= OnLevelStarted;
            Actions.LevelFinished -= OnLevelFinished;
        }

        private void OnLevelStarted()
        {
            // Activate camera at index 0 when the level starts
            ChangeCameraPriority(0);
        }

        private void OnLevelFinished()
        {
            // Activate camera at index 1 when the level finishes
            ChangeCameraPriority(1);
        }

        private void ChangeCameraPriority(int cameraIndex)
        {
            // Give the selected camera higher priority (10) and lower all others (0)
            for (int i = 0; i < cameraList.Count; i++)
            {
                cameraList[i].Priority = i == cameraIndex ? 10 : 0;
            }
        }
    }
}