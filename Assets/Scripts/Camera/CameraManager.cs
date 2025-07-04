using System.Collections.Generic;
using Cinemachine;
using Game_Management;
using UnityEngine;

namespace Camera
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private List<CinemachineVirtualCamera> cameraList;
        
        private void OnEnable()
        {
            
            Actions.LevelStarted += OnLevelStarted;
            Actions.LevelFinished += OnLevelFinished;
        }

        private void OnDisable()
        {
            Actions.LevelStarted -= OnLevelStarted;
            Actions.LevelFinished -= OnLevelFinished;
        }

        private void OnLevelStarted()
        {
            ChangeCameraPriority(0);
        }

        private void OnLevelFinished()
        {
            ChangeCameraPriority(1);
        }

        private void ChangeCameraPriority(int cameraIndex)
        {
            foreach (var camera in cameraList)
            {
                camera.Priority = cameraList[cameraIndex] == camera ? 10 : 0;
            }
        }
    }
}
