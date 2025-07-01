using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private int score = 0;

    [Header("Follow Target (invisible GameObject for camera & character)")]
    public GameObject followTarget;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Call this every time a new grid is placed to update the followTarget position
    public void UpdateFollowTargetPosition(Vector3 currentGridPos, float previousGridX)
    {
        if (followTarget != null)
        {
            // Keep the X position of previous grid (to stay centered on stack)
            Vector3 newPos = currentGridPos;
            newPos.x = previousGridX;
            followTarget.transform.position = newPos;
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log("Score: " + score);
        // TODO: Update UI if needed
    }

    public void ResetScore()
    {
        score = 0;
    }

    public int GetScore()
    {
        return score;
    }
}
