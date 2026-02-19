using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Mouvement de Base")]
    [SerializeField] private float forwardSpeed = 10f;
    [SerializeField] private float laneDistance = 3f;
    [SerializeField] private float laneChangeSpeed = 15f;

    [Header("Saut")]
    [SerializeField] private float jumpForce = 4f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Glissade")]
    [SerializeField] private float slideDuration = 1f;
    [SerializeField] private float slideHeight = 0.5f;

    private Rigidbody rb;
    private Vector3 direction;
    [SerializeField] private int targetLane = 1; // 0:G, 1:M, 2:D
    private bool isSliding = false;
    private bool isGrounded;

    [HideInInspector] public bool canTurn = false;

    private Coroutine laneChangeCoroutine;
    private Vector3 lateralOffset;
    private float currentLocalX = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Initialisation correcte selon la ligne de départ
        currentLocalX = (targetLane - 1) * laneDistance;
    }

    void Update()
    {
        CheckGround();
    }

    private void FixedUpdate()
    {
        Vector3 forwardMove = transform.forward * forwardSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + forwardMove + lateralOffset);
        lateralOffset = Vector3.zero;
    }

    void OnJump()
    {
        if (isGrounded)
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void OnChangeLane(InputValue value)
    {
        float val = value.Get<float>();

        if (canTurn)
        {
            if (val != 0)
            {
                float angle = val > 0 ? 90f : -90f;
                transform.Rotate(0, angle, 0);
                canTurn = false;
                currentLocalX = 0f;
                lateralOffset = Vector3.zero;
            }
        }
        else
        {
            if (val > 0 && targetLane < 2)
            {
                targetLane++;
                if (laneChangeCoroutine != null) StopCoroutine(laneChangeCoroutine);
                laneChangeCoroutine = StartCoroutine(ChangeLaneRoutine());
            }
            else if (val < 0 && targetLane > 0)
            {
                targetLane--;
                if (laneChangeCoroutine != null) StopCoroutine(laneChangeCoroutine);
                laneChangeCoroutine = StartCoroutine(ChangeLaneRoutine());
            }
        }
    }

    private IEnumerator ChangeLaneRoutine()
    {
        float targetX = (targetLane - 1) * laneDistance;

        // Boucle tant que le mouvement n'est pas terminé
        while (Mathf.Abs(targetX - currentLocalX) > 0.001f)
        {
            float newX = Mathf.MoveTowards(currentLocalX, targetX, laneChangeSpeed * Time.fixedDeltaTime);
            lateralOffset = transform.right * (newX - currentLocalX);
            currentLocalX = newX;
            yield return new WaitForFixedUpdate();
        }

        currentLocalX = targetX;
        lateralOffset = Vector3.zero;
    }
}