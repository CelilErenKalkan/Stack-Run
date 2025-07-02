using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gameplay Settings")]
    [SerializeField] private Transform finishLine;
    [SerializeField] private GameObject followTarget;

    private int currentLevel = 1;
    private int score = 0;

    public int CurrentLevel => currentLevel;
    public int Score => score;
    public Transform FinishLine => finishLine;
    public GameObject FollowTarget => followTarget;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void IncreaseLevel()
    {
        currentLevel++;
        Debug.Log($"Level increased to {currentLevel}");
    }

    public void ResetLevel()
    {
        currentLevel = 1;
        Debug.Log("Level reset to 1");
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"Score: {score}");
    }

    public void ResetScore() => score = 0;

    public void SetFinishLine(Transform newFinishLine)
    {
        finishLine = newFinishLine;
    }

    public bool ReachedFinishLine(float zPos)
    {
        return finishLine != null && (finishLine.position.z - zPos <= 0.5f);
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