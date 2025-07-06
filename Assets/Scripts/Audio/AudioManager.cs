using System.Collections.Generic;
using Data_Management;
using Game_Management;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Audio
{
    public class AudioManager : MonoBehaviour
    {
        private Pool _pool;
        public List<AudioClip> soundList;

        private int _perfectStreak = 0;
        private float _basePitch = 1f;
        private float _pitchIncreasePerPerfect = 0.05f;
        private float _maxPitch = 2f;

        private void OnEnable()
        {
            Actions.ButtonPush += OnButtonPush;
            Actions.LevelStarted += OnGameStart;
            Actions.StandardNote += OnStandardNote;
            Actions.PerfectNote += OnPerfectNote;
            Actions.LevelFinished += OnLevelFinished;
            Actions.LevelFailed += OnLevelFailed;
        }

        private void OnDisable()
        {
            Actions.ButtonPush -= OnButtonPush;
            Actions.LevelStarted -= OnGameStart;
            Actions.StandardNote -= OnStandardNote;
            Actions.PerfectNote -= OnPerfectNote;
            Actions.LevelFinished -= OnLevelFinished;
            Actions.LevelFailed -= OnLevelFailed;
        }

        private void Start()
        {
            _pool = Pool.Instance;
        }

        private void OnButtonPush()
        {
            PlaySound(0, _basePitch);
        }

        private void OnGameStart()
        {
            _perfectStreak = 0; // Reset streak on level start
            PlaySound(1, _basePitch);
        }

        private void OnStandardNote()
        {
            _perfectStreak = 0; // Reset streak
            var gridSoundIndex = Random.Range(0, 5);
            PlaySound(gridSoundIndex + 1, _basePitch);
        }

        private void OnPerfectNote()
        {
            _perfectStreak++;
            float pitch = Mathf.Min(_basePitch + (_pitchIncreasePerPerfect * _perfectStreak), _maxPitch);
            PlaySound(6, pitch);
        }

        private void OnLevelFinished()
        {
            PlaySound(7, _basePitch);
        }
        
        private void OnLevelFailed()
        {
            PlaySound(8, _basePitch);
        }

        private void PlaySound(int index, float pitch)
        {
            if (DataManager.gameData.isMuted) return;

            float time = soundList[index].length + 0.1f;
            var audioObject = _pool.SpawnObject(transform.position, PoolItemType.AudioSource, null, time);
            if (audioObject.TryGetComponent(out AudioSource audioSource))
            {
                audioSource.pitch = pitch / Time.timeScale;
                audioSource.clip = soundList[index];
                audioSource.Play();
            }
        }
    }
}
