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

        public TMP_Text levelText, scoreText;

        private Image _audioButtonImage;
        private Sprite _mute, _unmute;
        private TMP_Text _buttonStartText;

        private Animator _animator;

        private int score = 0;

        private void OnEnable()
        {
            if (gameObject.TryGetComponent(out Animator animator))
                _animator = animator;

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

        /// <summary>
        /// Increases score and triggers score animation.
        /// </summary>
        private void NewScore(GameObject grid)
        {
            score++;
            scoreText.text = score + "!";
            if (score > 0)
                _animator.Play("NewScore");
        }

        /// <summary>
        /// Starts the level, resets score and triggers animations.
        /// </summary>
        private void LevelStart()
        {
            score = -1;
            Actions.ButtonPush?.Invoke();
            Actions.LevelStarted?.Invoke();

            levelText.text = "Level " + DataManager.gameData.levelNo;
            GameUIAnimation(true);
        }

        /// <summary>
        /// Called when level is finished successfully.
        /// </summary>
        private void OnLevelFinished()
        {
            _buttonStartText.text = "NEXT LEVEL";
            GameUIAnimation(false);
        }

        /// <summary>
        /// Called when the level fails.
        /// </summary>
        private void OnLevelFailed()
        {
            _buttonStartText.text = "RESTART";
            GameUIAnimation(false);
        }

        /// <summary>
        /// Plays UI animation with a delay depending on game state.
        /// </summary>
        private void GameUIAnimation(bool isStarting)
        {
            StartCoroutine(delayedCallForUI(isStarting ? 0.1f : 3.0f));

            IEnumerator delayedCallForUI(float time)
            {
                yield return new WaitForSeconds(time);
                _animator.SetTrigger(isStarting ? "StartGame" : "EndGame");
            }
        }

        /// <summary>
        /// Toggles mute/unmute and saves preference.
        /// </summary>
        private void ChangeAudioMod()
        {
            Actions.ButtonPush?.Invoke();
            DataManager.gameData.isMuted = !DataManager.gameData.isMuted;
            SetAudio();
        }

        /// <summary>
        /// Updates audio icon and triggers audio state change event.
        /// </summary>
        private void SetAudio()
        {
            _audioButtonImage.sprite = DataManager.gameData.isMuted ? _mute : _unmute;
            Actions.AudioChanged?.Invoke(DataManager.gameData.isMuted);
            DataManager.SaveData();
        }

        /// <summary>
        /// Loads audio icons and sets current audio state.
        /// </summary>
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

        /// <summary>
        /// Sets up button click listeners and text references.
        /// </summary>
        private void SetButtons()
        {
            if (buttonStart.transform.GetChild(0).TryGetComponent(out TMP_Text text))
                _buttonStartText = text;

            buttonStart.onClick.AddListener(LevelStart);
            buttonAudio.onClick.AddListener(ChangeAudioMod);
        }
    }
}
