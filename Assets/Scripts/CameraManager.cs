using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [Header("Cible")]
    [SerializeField] private Transform player;

    [Header("Réglages")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2.5f, -7);
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float rotationX = 7f;

    void LateUpdate()
    {
        if (player == null) return;

        Quaternion playerYRotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
        Vector3 desiredPosition = player.position + (playerYRotation * offset);
        desiredPosition.y = offset.y;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(rotationX, player.eulerAngles.y, 0f), smoothSpeed * Time.deltaTime);
    }
}