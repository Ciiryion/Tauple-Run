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

    [Header("Score et Game Over")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("Limites de Terrain")]
    [SerializeField] private float fallDeathYThreshold = -5f;

    private Animator playerAnimator;

    private Rigidbody rb;
    private Vector3 direction;
    private int targetLane = 1; // 0:G, 1:M, 2:D
    private bool isSliding = false;
    private bool isGrounded;

    [HideInInspector] public bool canTurn = false;
    [HideInInspector] public bool isTurning = false;
    [HideInInspector] public bool isDead = false;

    private Vector3 lateralOffset;
    private float currentLocalX = 0f;

    private Coroutine changeLaneCo;

    private const string ISSLIDING = "isSliding";

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();

        // Initialisation correcte selon la ligne de départ
        currentLocalX = (targetLane - 1) * laneDistance;
    }

    void Update()
    {
        if (isDead) return;

        CheckGround();

        // Anti-triche : si le joueur tombe sous la limite, on le tue
        if (transform.position.y <= fallDeathYThreshold)
        {
            Debug.Log("L'IA a essayé de tricher en tombant !");
            Die();
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return; 

        playerAnimator.SetBool(ISSLIDING, false);
        
        // On récupère le multiplicateur de vitesse depuis le GameManager
        float currentSpeedMod = GameManager.Instance != null ? GameManager.Instance.CurrentSpeedMultiplier : 1f;

        // On applique l'accélération
        Vector3 forwardMove = transform.forward * forwardSpeed * currentSpeedMod * Time.fixedDeltaTime;
        
        rb.MovePosition(rb.position + forwardMove + lateralOffset);
        lateralOffset = Vector3.zero;
    }

    public void Die()
    {
        if (isDead) return; // Sécurité pour éviter d'appeler Die() plusieurs fois

        Debug.Log("Le joueur a perdu");
        isDead = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        if (scoreManager != null)
        {
            scoreManager.StopScore();
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    #region ACTIONS PUBLIQUES
    public void Jump()
    {
        if (isDead) return; 

        if (isGrounded)
            rb.AddForce(new Vector3(0, jumpForce, 0), ForceMode.Impulse);
    }

    public void MoveLeft()
    {
        if (isDead) return;

        if (canTurn)
        {
            ExecuteTurn(-90f);
        }
        else
        {
            if (isTurning) return;
            if (targetLane > 0)
            {
                targetLane--;
                if (changeLaneCo != null) StopCoroutine(changeLaneCo);
                changeLaneCo = StartCoroutine(ChangeLaneRoutine());
            }
        }
    }

    public void MoveRight()
    {
        if (isDead) return;

        if (canTurn)
        {
            ExecuteTurn(90f);
        }
        else
        {
            if (isTurning) return;
            if (targetLane < 2)
            {
                targetLane++;
                if (changeLaneCo != null) StopCoroutine(changeLaneCo);
                changeLaneCo = StartCoroutine(ChangeLaneRoutine());
            }
        }
    }

    public void SlideAction()
    {
        if (isDead) return;

        if (!isSliding && isGrounded)
        {
            playerAnimator.SetBool(ISSLIDING, true);
            StartCoroutine(SlideCoroutine()); // Coroutine renommée
        }
    }

    private void ExecuteTurn(float angle)
    {
        Quaternion rotationToAdd = Quaternion.Euler(0, angle, 0);
        Quaternion newRotation = rb.rotation * rotationToAdd;
        rb.rotation = newRotation;
        transform.rotation = newRotation;
        canTurn = false;
        currentLocalX = 0f;
        lateralOffset = Vector3.zero;

        turnEvent?.Invoke(transform.forward);

        // Récompense pour avoir réussi un virage
        RunnerAgent agent = GetComponent<RunnerAgent>();
        if (agent != null)
        {
            // boost positif qui va forcer l'IA à chercher les virages
            agent.AddReward(0.5f); // Valeur modifiable
        }
    }
    #endregion

    #region INPUTS UNITY
    void OnJump()
    {
        Jump();
    }

    void OnSlide()
    {
        SlideAction();
    }

    void OnChangeLane(InputValue value)
    {
        float val = value.Get<float>();

        if (Mathf.Approximately(val, 0)) return; 

        if (val > 0)
        {
            MoveRight();
        }
        else if (val < 0)
        {
            MoveLeft();
        }
    }
    #endregion

    #region COROUTINES + RESET
    private IEnumerator ChangeLaneRoutine()
    {
        float targetX = (targetLane - 1) * laneDistance;

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

    private IEnumerator SlideCoroutine()
    {
        // Debug.Log("Start Slide");
        isSliding = true;

        GetComponent<CapsuleCollider>().height = 0.5f;
        GetComponent<CapsuleCollider>().center = new Vector3(0, -0.5f, 0);

        yield return new WaitForSeconds(slideAnimationClip.length);

        GetComponent<CapsuleCollider>().height = 2f;
        GetComponent<CapsuleCollider>().center = new Vector3(0, 0, 0);

        isSliding = false;
        // Debug.Log("End Slide");
    }

    public void ReturnToCenter(Transform centerCheck)
    {
        transform.position = centerCheck.position;
        targetLane = 1;
        lateralOffset = Vector3.zero;
        if (changeLaneCo != null)
        {
            StopCoroutine(changeLaneCo);
        }
    }

    public void ResetPlayer(Vector3 startPosition, Quaternion startRotation)
    {
        // 1. Réinitialisation des états
        isDead = false;
        canTurn = false;
        isTurning = false;
        targetLane = 1; 
        currentLocalX = 0f;
        lateralOffset = Vector3.zero;

        if (changeLaneCo != null) StopCoroutine(changeLaneCo);
        
        // 2. Réinitialisation de la physique et de l'animation
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        transform.SetPositionAndRotation(startPosition, startRotation);
        rb.position = startPosition;
        rb.rotation = startRotation;

        playerAnimator.SetBool(ISSLIDING, false);
    }
    #endregion
}