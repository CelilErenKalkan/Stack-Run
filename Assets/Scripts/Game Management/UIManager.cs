using System.Collections;
using Data_Management;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Management
{
    public class UIManager : MonoBehaviour
    {
        public Button buttonGameStart, buttonAudio;

        private Image _audioButtonImage;
        private Sprite _mute, _unmute;

        private Animator _animator;

        private void OnEnable()
        {
            if (gameObject.TryGetComponent(out Animator animator)) _animator = animator;
            
            Actions.LevelFinished += OnLevelFinished;
        }

        private void OnDisable()
        {
            Actions.LevelFinished -= OnLevelFinished;
        }

        private void Start()
        {
            SetSprites();
            SetButtons();
        }

        private void LevelStart()
        {
            Actions.ButtonPush?.Invoke();
            Actions.LevelStarted?.Invoke();
            GameUIAnimation(true);
        }

        private void OnLevelFinished()
        {
            GameUIAnimation(false);
        }

        private void GameUIAnimation(bool isStarting)
        {
            StartCoroutine(delayedCallForUI(isStarting? 0.1f : 3.0f));

            IEnumerator delayedCallForUI(float time)
            {
                yield return new WaitForSeconds(time);
                _animator.SetTrigger(isStarting ? "StartGame" : "EndGame");
            }
        }

        private void ChangeAudioMod()
        {
            Actions.ButtonPush?.Invoke();
            DataManager.gameData.isMuted = !DataManager.gameData.isMuted;
            SetAudio();
        }

        private void SetAudio()
        {
            _audioButtonImage.sprite = DataManager.gameData.isMuted ? _mute : _unmute;
            Actions.AudioChanged?.Invoke(DataManager.gameData.isMuted);
            DataManager.SaveData();
        }

        private void SetSprites()
        {
            _mute = Resources.Load<Sprite>("UI/ui_icon_main_menu_mute");
            _unmute = Resources.Load<Sprite>("UI/ui_icon_main_menu_unmute");

            if (_mute == null || _unmute == null)
            {
                Debug.LogError("One or more sprites failed to load. Check paths and ensure assets are in Resources folder.");
            }

            _audioButtonImage = buttonAudio.transform.GetChild(0).GetComponent<Image>();
            SetAudio();
        }

        private void SetButtons()
        {
            buttonGameStart.onClick.AddListener(() => LevelStart());
            buttonAudio.onClick.AddListener(ChangeAudioMod);
        }
    }
}
