using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float gravity = -20f;

    [Header("Look Settings")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float upLowerLimit = -80f;
    [SerializeField] private float upperUpperLimit = 80f;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private float currentSpeed;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate camera up and down (pitch)
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, upLowerLimit, upperUpperLimit);
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }

        // Rotate entire player left and right (yaw)
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Determine if running (Left Shift)
        bool isRunning = Input.GetKey(KeyCode.LeftShift) && moveZ > 0;
        currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Gravity handling
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // Feed speed into Animator Blend Tree (0 = Idle, Walk Speed = Walk, Run Speed = Run)
        if (animator != null)
        {
            // Calculate horizontal magnitude for animator
            float animationSpeed = new Vector3(moveX, 0, moveZ).magnitude * (isRunning ? 2f : 1f);
            animator.SetFloat("Speed", animationSpeed, 0.1f, Time.deltaTime);
        }
    }
}