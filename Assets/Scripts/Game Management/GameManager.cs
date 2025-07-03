using Data_Management;
using Game_Management;
using Grid_Mechanic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gameplay Settings")]
    [SerializeField] private Transform finishLine;
    [SerializeField] private GameObject followTarget;
    public Transform FinishLine => finishLine;
    public GameObject FollowTarget => followTarget;

    public bool isLevelStarted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DataManager.LoadData();
        DataManager.LoadLevelOrder();
    }

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

    private void Update()
    {
        if (isLevelStarted)
            GridManager.HandleClickInput();
    }

    private void OnLevelStarted()
    {
        isLevelStarted = true;
        Pool.Instance.SpawnObject(new Vector3(0f, 0.2f, 0f), PoolItemType.Chibi, null);
    }
    
    private void OnLevelFinished()
    {
        isLevelStarted = false;
        GridManager.UpdateLevelEnd();
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