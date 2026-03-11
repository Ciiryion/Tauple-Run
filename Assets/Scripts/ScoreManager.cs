using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text scoreText;

    [Header("Score Settings")]
    [Tooltip("Multiplicateur pour définir à quelle vitesse le score augmente par seconde")]
    public float scoreMultiplier = 10f; 

    private float currentScore = 0f;
    private bool isPlayerDead = false;

    private void Update()
    {
        // On n'augmente le score que si le joueur est en vie
        if (!isPlayerDead)
        {
            currentScore += Time.deltaTime * scoreMultiplier;
            UpdateScoreUI();
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            // Arrondir le score
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
}