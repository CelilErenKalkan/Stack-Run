using System.Collections.Generic;
using Data_Management;
using Game_Management;
using UnityEngine;

namespace Audio
{
    public class BackgroundMusicManager : MonoBehaviour
    {
        public AudioSource audioSource;
        public List<AudioClip> musicClip;

        [SerializeField] private float volume;

        private void OnEnable()
        {
            Actions.AudioChanged += SetAudioMod;
            Actions.LevelStarted += PlayInGameMusic;
            Actions.LevelFinished += PlaySuccessMusic;

            SetAudioMod(DataManager.gameData.isMuted);
        }
        
        private void OnDisable()
        {
            Actions.AudioChanged -= SetAudioMod;
            Actions.LevelStarted -= PlayInGameMusic;
            Actions.LevelFinished -= PlayMainMenuMusic;
        }
        
        private void Start()
        {
            PlayMainMenuMusic();
        }
        
        private void SetAudioMod(bool isMuted)
        {
            audioSource.volume = isMuted ? 0 : volume;
        }

        private void PlayInGameMusic()
        {
            audioSource.pitch = 1f / Time.timeScale;
            audioSource.clip = musicClip[1];
            audioSource.Play();
        }

        private void PlayMainMenuMusic()
        {
            audioSource.pitch = 1f / Time.timeScale;
            audioSource.clip = musicClip[0];
            audioSource.Play();
        }
        
        private void PlaySuccessMusic()
        {
            audioSource.pitch = 1f / Time.timeScale;
            audioSource.clip = musicClip[2];
            audioSource.Play();
        }
    }
}