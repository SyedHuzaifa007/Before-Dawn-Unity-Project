using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    //new input system
    private PlayerControls controls;
    private Vector2 movementInput;
    private bool jumpInput;
    private bool throwInput;
    private bool targetInput;

    public GameObject ShurikenSpawn;
    public GameObject Shuriken;
    public Transform playerBody;
    public Transform groundCheck;
    public LayerMask ground;
    public PlayerStats playerStats;
    public AudioSource throw1, throw2;

    public float movementSpeed = 15f;
    public float throwForce = 150f;
    public float rotationSpeed = 50f;
    public float groundDistance = 0.4f;
    public float jumpHeight = 4f;

    private bool isGrounded;
    private Rigidbody rb;
    private Animator anim;
    private RaycastHit hit;
    private bool canThrow = false;
    private float throwRange = 30f;
    private GameObject Target;
    private GameObject PreviousTarget;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        rb.freezeRotation = true;

        controls = new PlayerControls();

        controls.Player.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => movementInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => jumpInput = true;
        controls.Player.Throw.performed += ctx => throwInput = true;
        controls.Player.Target.performed += ctx => targetInput = true;
    }


    //void Start()
    //{
    //    rb = GetComponent<Rigidbody>();
    //    anim = GetComponent<Animator>();
    //    rb.freezeRotation = true; // Prevent physics from rotating the player
    //}

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();


    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, ground);
        anim.SetBool("Walk", false);

        HandleJump();
        HandleMovement();
        HandleThrowing();
        HandleTargeting();
    }

    void HandleJump()
    {
        if (jumpInput && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), rb.linearVelocity.z);
            anim.Play("Standing Jump");
        }
        jumpInput = false;
    }



    void HandleMovement()
    {
        Quaternion targetRotation = transform.rotation;
        bool isMoving = movementInput != Vector2.zero;

        if (movementInput.y < 0)
            targetRotation = Quaternion.Euler(0, 0f, 0);
        else if (movementInput.y > 0)
            targetRotation = Quaternion.Euler(0, 180f, 0);
        else if (movementInput.x > 0)
            targetRotation = Quaternion.Euler(0, 270f, 0);
        else if (movementInput.x < 0)
            targetRotation = Quaternion.Euler(0, 90f, 0);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (isMoving)
        {
            Vector3 moveDirection = transform.forward * movementSpeed;
            rb.linearVelocity = new Vector3(moveDirection.x, rb.linearVelocity.y, moveDirection.z);
            anim.SetBool("Walk", true);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            anim.SetBool("Walk", false);
        }
    }


    void HandleThrowing()
    {
        if (throwInput && canThrow && playerStats != null && playerStats.shuriken > 0)
        {
            playerStats.UseShuriken();
            anim.SetTrigger("Throw");
            throw1.PlayOneShot(throw1.clip);

            GameObject thrownShuriken = Instantiate(Shuriken, ShurikenSpawn.transform.position, ShurikenSpawn.transform.rotation);

            if (Target == null) return;

            Vector3 direction = Target.transform.position - thrownShuriken.transform.position;
            Rigidbody shurikenRb = thrownShuriken.GetComponent<Rigidbody>();
            if (shurikenRb != null)
                shurikenRb.AddForce(direction * throwForce);

            canThrow = false;

            Canvas targetCanvas = Target.GetComponentInChildren<Canvas>();
            if (targetCanvas != null)
                targetCanvas.enabled = false;
        }
        throwInput = false;
    }


    void HandleTargeting()
    {
        if (targetInput)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, throwRange) && hit.collider.CompareTag("Target"))
            {
                TorchController torch = hit.collider.GetComponentInParent<TorchController>();
                if (torch != null && !torch.isExtinguished)
                {
                    PreviousTarget = Target;
                    Target = hit.collider.gameObject;
                    canThrow = true;

                    if (PreviousTarget != null)
                        PreviousTarget.GetComponentInChildren<Canvas>().enabled = false;
                    Target.GetComponentInChildren<Canvas>().enabled = true;
                }
            }
        }
        targetInput = false;
    }

}