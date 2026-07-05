using UnityEngine;
using TMPro;

public class ScoreZone : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private string scorePrefix = "Score: ";

    [Header("Trigger Settings")]
    [Tooltip("If enabled, only colliders with this tag will count toward score.")]
    public bool requireTag = true;
    [Tooltip("Tag required for objects to be counted when they pass through the trigger.")]
    public string targetTag = "ScoreTarget";

    private int score = 0;

    private void Start()
    {
        UpdateScoreText();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (requireTag && !other.CompareTag(targetTag))
        {
            return;
        }

        score++;
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText == null)
        {
            Debug.LogWarning("ScoreZone: scoreText is not assigned.", this);
            return;
        }

        scoreText.text = scorePrefix + score;
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreText();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateScoreText();
    }
}
