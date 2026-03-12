using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.InputSystem;

public class RunnerAgent : Agent
{
    [Header("Références")]
    [SerializeField] private PlayerController player;

    // 1. DÉMARRAGE DE L'ÉPISODE
    public override void OnEpisodeBegin()
    {
        // On demande au GameManager de réinitialiser tout le terrain et le joueur instantanément
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetGame();
        }
    }

    // 2. OBSERVATIONS
    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Position X
        sensor.AddObservation(player.transform.position.x);

        // 2. Vitesse actuelle
        if (GameManager.Instance != null)
        {
            sensor.AddObservation(GameManager.Instance.CurrentSpeedMultiplier);
        }
        else
        {
            sensor.AddObservation(1f); // Sécurité au cas où
        }

        // Est-ce qu'on a le droit de tourner ? (1 pour Oui, 0 pour Non)
        sensor.AddObservation(player.canTurn ? 1f : 0f);
    }

    // 3. ACTIONS ET RÉCOMPENSES
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Si le joueur est mort, on arrête de traiter les actions
        if (player.isDead) return;

        // On récupère l'action choisie par le réseau de neurones (une valeur entre 0 et 4)
        int action = actions.DiscreteActions[0];

        switch (action)
        {
            case 0: 
                // Ne rien faire
                break;
            case 1: 
                player.MoveLeft(); 
                break;
            case 2: 
                player.MoveRight(); 
                break;
            case 3: 
                player.Jump(); 
                break;
            case 4: 
                player.SlideAction(); 
                break;
        }

        // Récompense positive : on donne un tout petit peu de points à chaque frame où l'IA est en vie
        // > l'encourage à survivre le plus longtemps possible.
        AddReward(0.01f);
    }

    // On vérifie la mort dans le FixedUpdate
    private void FixedUpdate()
    {
        // Si l'IA percute un obstacle, on la punit sévèrement et on termine l'essai
        if (player.isDead)
        {
            SetReward(-1f); // Punition
            EndEpisode();
        }
    }

    // 4. TEST MANUEL (Heuristic)
    // Cette fonction te permet de tester l'agent avec son clavier avant de lancer l'entraînement
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        
        // Par défaut, l'action est 0 (ne rien faire)
        discreteActions[0] = 0;

        // On s'assure qu'un clavier est bien connecté pour éviter d'autres erreurs
        if (Keyboard.current == null) return;

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame || Keyboard.current.qKey.wasPressedThisFrame)
            discreteActions[0] = 1;
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.dKey.wasPressedThisFrame)
            discreteActions[0] = 2;
        else if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            discreteActions[0] = 3;
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
            discreteActions[0] = 4;
    }
}