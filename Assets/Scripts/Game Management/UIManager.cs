using System.Collections;
using Data_Management;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game_Management
{
    public class UIManager : MonoBehaviour
    {
        public Button buttonStart, buttonAudio;

        private Image _audioButtonImage;
        private Sprite _mute, _unmute;
        private TMP_Text _buttonStartText;
        public TMP_Text levelText, scoreText;

        private int score = 0;

        private Animator _animator;

        private void OnEnable()
        {
            if (gameObject.TryGetComponent(out Animator animator)) _animator = animator;

            Actions.LevelFinished += OnLevelFinished;
            Actions.LevelFailed += OnLevelFailed;
            Actions.SetNextGrid += NewScore;
        }

        private void OnDisable()
        {
            Actions.LevelFinished -= OnLevelFinished;
            Actions.LevelFailed -= OnLevelFailed;
        }

        private void Start()
        {
            SetSprites();
            SetButtons();
        }

        private void NewScore(GameObject grid)
        {
            score++;
            scoreText.text = score + "!";
            _animator.Play("NewScore");
        }

        private void LevelStart()
        {
            Actions.ButtonPush?.Invoke();
            Actions.LevelStarted?.Invoke();
            levelText.text = "Level " + DataManager.gameData.levelNo;
            GameUIAnimation(true);
        }

        private void OnLevelFinished()
        {
            _buttonStartText.text = "NEXT LEVEL";
            GameUIAnimation(false);
        }
        
        private void OnLevelFailed()
        {
            _buttonStartText.text = "RESTART";
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
            if (buttonStart.transform.GetChild(0).TryGetComponent(out TMP_Text text)) _buttonStartText = text;
            buttonStart.onClick.AddListener(() => LevelStart());
            buttonAudio.onClick.AddListener(ChangeAudioMod);
        }
    }
}
