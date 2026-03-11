using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Score Settings")]
    [Tooltip("Multiplicateur de base pour le score par seconde")]
    public float baseScoreMultiplier = 10f; 

    private float currentScore = 0f;
    private bool isPlayerDead = false;

    private void Update()
    {
        if (!isPlayerDead && GameManager.Instance != null)
        {
            // Le score augmente de plus en plus vite en fonction de la vitesse du jeu
            float speedFactor = GameManager.Instance.CurrentSpeedMultiplier;
            currentScore += Time.deltaTime * baseScoreMultiplier * speedFactor;
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + Mathf.FloorToInt(currentScore).ToString();
        }
        else
        {
            Debug.LogWarning("ScoreManager: Le champ Score Text n'est pas assigné dans l'inspecteur !");
        }
    }

    public void StopScore()
    {
        isPlayerDead = true;
    }

    // Ajouté pour préparer les resets de l'IA
    public void ResetScore()
    {
        currentScore = 0f;
        isPlayerDead = false;
    }
}