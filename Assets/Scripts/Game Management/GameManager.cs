using Data_Management;
using Grid_Mechanic;
using UnityEngine;

namespace Game_Management
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        [Header("Gameplay Settings")]
        [SerializeField] private Transform finishLine;
        [SerializeField] private GameObject followTarget;
        [SerializeField] private GameObject chibi;

        public Transform FinishLine => finishLine;
        public GameObject FollowTarget => followTarget;
        public GameObject Chibi => chibi;

        public bool isLevelStarted;

        private void Awake()
        {
            // Singleton pattern: ensure only one instance exists
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // Load player and level data from disk
            DataManager.LoadData();
            DataManager.LoadLevelOrder();
        }

        private void OnEnable()
        {
            Actions.LevelStarted += OnLevelStarted;
            Actions.LevelFinished += OnLevelFinished;
            Actions.LevelFailed += OnLevelFailed;
        }

        private void OnDisable()
        {
            Actions.LevelStarted -= OnLevelStarted;
            Actions.LevelFinished -= OnLevelFinished;
            Actions.LevelFailed -= OnLevelFailed;
        }

        private void Update()
        {
            // Listen for input while the level is active
            if (isLevelStarted)
                GridManager.HandleClickInput();
        }

        private void OnLevelStarted()
        {
            // Respawn chibi character each time a level starts
            if (chibi != null)
            {
                Pool.Instance.DeactivateObject(chibi, PoolItemType.Chibi);
            }
        
        
            SpawnChibi();
            isLevelStarted = true;
        }

        private void OnLevelFinished()
        {
            isLevelStarted = false;
            GridManager.UpdateLevelEnd(true);
            UpdateFollowTarget(Vector3.zero, 0f);
        }

        private void OnLevelFailed()
        {
            isLevelStarted = false;
            GridManager.UpdateLevelEnd(false);
            UpdateFollowTarget(Vector3.zero, 0f);
        }

        private void SpawnChibi()
        {
            chibi = Pool.Instance.SpawnObject(new Vector3(0f, 0.2f, 0f), PoolItemType.Chibi, null);
            chibi.transform.eulerAngles = Vector3.zero;

            // Reset physics properties if Rigidbody exists
            if (chibi.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.velocity = Vector3.zero;
            }
        }

        public void SetFinishLine(Transform newFinishLine)
        {
            finishLine = newFinishLine;
        }

        public void UpdateFollowTarget(Vector3 newGridPos, float previousX)
        {
            if (followTarget != null)
            {
                newGridPos.x = previousX;
                followTarget.transform.position = newGridPos;
            }
        }
    }
}
