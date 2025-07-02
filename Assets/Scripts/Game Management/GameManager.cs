using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int currentLevel = 1;
    public int GetLevel() => currentLevel;

    public GameObject followTarget;

    public Transform finishLine;

    public float currentFinishZ { get; private set; }

    [Header("Score")]
    public int score = 0;

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

    public void ResetScore()
    {
        score = 0;
    }

    public void UpdateFollowTargetPosition(Vector3 currentGridPos, float previousGridX)
    {
        if (followTarget != null)
        {
            Vector3 newPos = currentGridPos;
            newPos.x = previousGridX;
            followTarget.transform.position = newPos;
        }
    }

    public bool CheckFinisLine(float distanceZ)
    {
        return finishLine.position.z - distanceZ <= 0.5f;
    }
}
