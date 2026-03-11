using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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
    [SerializeField] private AnimationClip slideAnimationClip;

    [Header("Virage")]
    [SerializeField]
    private UnityEvent<Vector3> turnEvent;

    private Animator playerAnimator;

    private Rigidbody rb;
    private Vector3 direction;
    private int targetLane = 1; // 0:G, 1:M, 2:D
    private bool isSliding = false;
    private bool isGrounded;

    [HideInInspector] public bool canTurn = false;
    [HideInInspector] public bool isTurning = false;

    private Vector3 lateralOffset;
    private float currentLocalX = 0f;

    private Coroutine changeLaneCo;

    private const string ISSLIDING = "isSliding";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();

        // Initialisation correcte selon la ligne de d�part
        currentLocalX = (targetLane - 1) * laneDistance;
    }

    void Update()
    {
        CheckGround();
    }

    private void FixedUpdate()
    {
        playerAnimator.SetBool(ISSLIDING, false);
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

        if (Mathf.Approximately(val, 0)) return; //La fonction n'est pas utilis� quand on rel�che la touche


        if (canTurn)
        {   
            // Gère le virage
            if (val != 0)
            {
                float angle = val > 0 ? 90f : -90f;
                Quaternion rotationToAdd = Quaternion.Euler(0, angle, 0);
                Quaternion newRotation = rb.rotation * rotationToAdd;
                rb.rotation = newRotation;
                transform.rotation = newRotation;
                canTurn = false;
                currentLocalX = 0f;
                lateralOffset = Vector3.zero;

                // Déclenche l'événement de virage
                turnEvent?.Invoke(transform.forward);
            }
        }
        else
        {
            if (isTurning) return;
            if (val > 0 && targetLane < 2)
            {
                targetLane++;
                if (changeLaneCo != null) StopCoroutine(changeLaneCo);
                changeLaneCo = StartCoroutine(ChangeLaneRoutine());
            }
            else if (val < 0 && targetLane > 0)
            {
                targetLane--;
                if (changeLaneCo != null) StopCoroutine(changeLaneCo);
                changeLaneCo = StartCoroutine(ChangeLaneRoutine());
            }
        }
    }

    private IEnumerator ChangeLaneRoutine()
    {
        float targetX = (targetLane - 1) * laneDistance;

        // Boucle tant que le mouvement n'est pas termin�
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

    public void returnToCenter(Transform centerCheck)
    {
        transform.position = centerCheck.position;
        targetLane = 1;
        lateralOffset = Vector3.zero;
        if (changeLaneCo != null)
        {
            StopCoroutine(changeLaneCo);
        }
    }

    void OnSlide()
    {
        if(!isSliding && isGrounded)
        {
            //Debug.Log("Slide");
            playerAnimator.SetBool(ISSLIDING, true);
            StartCoroutine(Slide());
        }
    }

    private IEnumerator Slide()
    {
        Debug.Log("Start Slide");
        isSliding = true;

        GetComponent<CapsuleCollider>().height = 0.5f;
        GetComponent<CapsuleCollider>().center = new Vector3(0, -0.5f, 0);

        yield return new WaitForSeconds(slideAnimationClip.length);

        GetComponent<CapsuleCollider>().center = new Vector3(0, 0, 0);
        GetComponent<CapsuleCollider>().height = 2f;

        isSliding = false;
        Debug.Log("End Slide");
    }
}