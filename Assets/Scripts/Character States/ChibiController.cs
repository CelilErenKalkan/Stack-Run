using Character;
using Game_Management;
using UnityEngine;

namespace Assets
{
    public class ChibiController : MonoBehaviour
    {
        private Animator anim;
        private State currentState;
        private GameManager _gameManager;
        
        [SerializeField] private float forwardSpeed = 0.5f;
        [SerializeField] private float followSpeed = 2.5f;
        [SerializeField] private float finishDistanceThreshold = 0.2f;

        // Start is called before the first frame update
        void Start()
        {
            anim = GetComponent<Animator>();
            _gameManager = GameManager.Instance;
            currentState = new Run(anim, GetComponent<ChibiController>());
        }

        // Update is called once per frame
        void Update()
        {
            currentState = currentState.Process();
        }
        
        private void OnEnable()
        {
            Actions.LevelFinished += OnLevelFinished;
        }

        private void OnDisable()
        {
            Actions.LevelFinished -= OnLevelFinished;
        }

        private void OnLevelFinished()
        {
            transform.position = Vector3.zero;
        }
        
        public void MoveForward()
        {
            Vector3 position = transform.position;

            // Move forward on Z
            position.z += forwardSpeed * Time.deltaTime;

            // Follow X of followTarget
            position.x = Mathf.Lerp(position.x, _gameManager.FollowTarget.transform.position.x, followSpeed * Time.deltaTime);

            transform.position = position;
        }

        public bool IsNearFinishLine()
        {
            if (_gameManager == null || _gameManager.FinishLine == null)
                return false;

            float distance = Vector3.Distance(transform.position, _gameManager.FinishLine.position);
            return distance <= finishDistanceThreshold;
        }
    }
}