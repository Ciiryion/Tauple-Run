using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton pour y accéder facilement de n'importe où via GameManager.Instance
    public static GameManager Instance { get; private set; }

    [Header("Speed Settings")]
    public float initialSpeedMultiplier = 1f;
    public float maxSpeedMultiplier = 5f;
    [Tooltip("À quelle vitesse le jeu accélère par seconde")]
    public float speedIncreaseRate = 0.02f; 

    // Variable lue par le Player et le ScoreManager
    public float CurrentSpeedMultiplier { get; private set; }

    private bool isGameRunning = true;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        CurrentSpeedMultiplier = initialSpeedMultiplier;
    }

    private void Update()
    {
        // Augmentation progressive de la vitesse au fil du temps
        if (isGameRunning && CurrentSpeedMultiplier < maxSpeedMultiplier)
        {
            CurrentSpeedMultiplier += speedIncreaseRate * Time.deltaTime;
        }
    }

    public void GameOver()
    {
        isGameRunning = false;
    }

    // Indispensable pour l'IA en renforcement : il faudra réinitialiser l'environnement à chaque nouvel essai (épisode)
    public void ResetGame()
    {
        CurrentSpeedMultiplier = initialSpeedMultiplier;
        isGameRunning = true;
    }
}