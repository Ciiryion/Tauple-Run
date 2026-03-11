using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // Gère les obstacles physiques (comme les murs ou les barrières)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            KillPlayer(collision.gameObject);
        }
    }

    // Gère les obstacles immatériels ou les zones de vide (comme les trous)
    // Nécessite un collider avec "Is Trigger" coché !
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            KillPlayer(other.gameObject);
        }
    }

    private void KillPlayer(GameObject playerObject)
    {
        PlayerController player = playerObject.GetComponent<PlayerController>();
        
        // On vérifie que le composant existe et que le joueur n'est pas déjà mort
        if (player != null && !player.isDead)
        {
            player.Die();
        }
    }
}