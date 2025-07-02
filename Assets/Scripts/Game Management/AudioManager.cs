using System;
using System.Collections.Generic;
using Data_Management;
using UnityEngine;

namespace Game_Management
{
    public class AudioManager : MonoBehaviour
    {
        private Pool _pool;
        public List<AudioClip> soundList;

        private void OnEnable()
        {
            Actions.LevelFinished += OnLevelFinished;
            Actions.SetNextGrid += OnSetNextGrid;
        }
        
        private void OnDisable()
        {
            Actions.LevelFinished -= OnLevelFinished;
            Actions.SetNextGrid -= OnSetNextGrid;
        }

        private void Start()
        {
            _pool = Pool.Instance;
        }

        private void OnLevelFinished()
        {
            PlaySound(0);
        }

        private void OnSetNextGrid(GameObject grid)
        {
            PlaySound(1);
        }

        private void PlaySound(int index)
        {
            if (DataManager.gameData.isMuted) return;
            
            float time = soundList[index].length + 0.1f;
            var audioObject = _pool.SpawnObject(transform.position, PoolItemType.AudioSource, null, time);
            if (audioObject.TryGetComponent(out AudioSource audioSource))
            {
                audioSource.pitch = 1f / Time.timeScale;
                audioSource.clip = soundList[index];
                audioSource.Play();
            }
        }
    }
    
}
