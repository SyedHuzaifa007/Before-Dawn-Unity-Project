using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerAnimatorController : MonoBehaviour
{
    private Animator animator;
    private CharacterController controller;

    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = 9.8f;
    public float groundCheckDistance = 0.5f;

    private Vector3 moveDirection = Vector3.zero;
    private bool isWalking = false;
    private bool isRunning = false;

    private bool isPushingOrPulling = false;

    private Transform cam;

    void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        cam = Camera.main.transform;
    }

    void Update()
    {
        if (isPushingOrPulling)
            return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.S)) vertical = -1f;
        if (Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.A)) horizontal = -1f;

        Vector3 inputDirection = new Vector3(horizontal, 0, vertical).normalized;
        isRunning = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && inputDirection.magnitude > 0;

        // Convert input to camera-relative movement
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * inputDirection.z + camRight * inputDirection.x;

        bool shouldWalk = inputDirection.magnitude > 0.1f;

        // Animations
        animator.SetBool("isWalking", shouldWalk);
        animator.SetBool("isRunning", isRunning);

        // Jump
        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetTrigger("Jump");
            moveDirection.y = Mathf.Sqrt(jumpHeight * 2 * gravity);
        }

        // Pull
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("Pull");
            StartCoroutine(TemporarilyDisableMovement(1.5f));
            return;
        }

        // Push
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Push");
            StartCoroutine(TemporarilyDisableMovement(1.5f));
            return;
        }

        // Face move direction
        if (moveDir != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
        }

        // Movement
        if (shouldWalk)
        {
            moveDir.Normalize();
            moveDirection.x = moveDir.x * (isRunning ? runSpeed : walkSpeed);
            moveDirection.z = moveDir.z * (isRunning ? runSpeed : walkSpeed);
        }
        else
        {
            moveDirection.x = 0f;
            moveDirection.z = 0f;
        }

        // Gravity
        if (!IsGrounded())
            moveDirection.y -= gravity * Time.deltaTime;
        else
            moveDirection.y = Mathf.Min(moveDirection.y, 0f);

        controller.Move(moveDirection * Time.deltaTime);
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);
    }

    System.Collections.IEnumerator TemporarilyDisableMovement(float duration)
    {
        isPushingOrPulling = true;
        yield return new WaitForSeconds(duration);
        isPushingOrPulling = false;
    }
}
