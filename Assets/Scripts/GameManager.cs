using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Speed Settings")]
    public float initialSpeedMultiplier = 1f;
    public float maxSpeedMultiplier = 3f;
    public float speedIncreaseRate = 0.02f; 

    [Header("References for Reset")]
    [SerializeField] private InfiniteTerrain terrainManager;
    [SerializeField] private PlayerController player;
    [SerializeField] private ScoreManager scoreManager;

    private Vector3 playerStartPosition;
    private Quaternion playerStartRotation;

    public float CurrentSpeedMultiplier { get; private set; }
    private bool isGameRunning = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        CurrentSpeedMultiplier = initialSpeedMultiplier;
    }

    private void Start()
    {
        // On sauvegarde la position initiale du joueur pour pouvoir l'y replacer plus tard
        if (player != null)
        {
            playerStartPosition = player.transform.position;
            playerStartRotation = player.transform.rotation;
        }
    }

    private void Update()
    {
        if (isGameRunning && CurrentSpeedMultiplier < maxSpeedMultiplier)
        {
            CurrentSpeedMultiplier += speedIncreaseRate * Time.deltaTime;
        }
    }

    public void GameOver()
    {
        isGameRunning = false;

        Debug.Log("GameOver");
        
        // Temporaire : Pour tester toi-même sans l'IA, on relance le jeu après 2 secondes
        Invoke("ResetGame", 2f); 
    }

    public void ResetGame()
    {
        Debug.Log("Resetting...");
        // On réinitialise la vitesse globale
        CurrentSpeedMultiplier = initialSpeedMultiplier;
        isGameRunning = true;

        // On réinitialise les autres systèmes
        if (scoreManager != null) scoreManager.ResetScore();
        if (terrainManager != null) terrainManager.ResetTerrain();
        if (player != null) player.ResetPlayer(playerStartPosition, playerStartRotation);
    }
}