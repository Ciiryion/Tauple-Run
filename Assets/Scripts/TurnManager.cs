using UnityEngine;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private bool isEnter = true; // True = Enter; False = Exit
    [SerializeField] private Transform centerCheck;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            if(isEnter)
            {
                Debug.Log("Virage");
                other.GetComponent<PlayerController>().canTurn = true;
            }
            else
            {
                Debug.Log("Exit");
                other.GetComponent<PlayerController>().returnToCenter(centerCheck);
            }
        }
    }
}
