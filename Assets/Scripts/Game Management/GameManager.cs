using System;
using Data_Management;
using Game_Management;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Gameplay Settings")]
    [SerializeField] private Transform finishLine;
    [SerializeField] private GameObject followTarget;
    public Transform FinishLine => finishLine;
    public GameObject FollowTarget => followTarget;

    private bool isLevelStarted;

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
    }

    private void OnDisable()
    {
        Actions.LevelStarted -= OnLevelStarted;
    }

    private void Update()
    {
        if (isLevelStarted)
            GridManager.HandleClickInput();
    }

    private void OnLevelStarted()
    {
        isLevelStarted = true;
    }

    public void LevelFinished()
    {
        DataManager.NewLevel();
    }

    public void SetFinishLine(Transform newFinishLine)
    {
        finishLine = newFinishLine;
    }

    public bool isReachedFinishLine(float zPos)
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