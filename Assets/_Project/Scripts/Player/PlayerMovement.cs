using UnityEngine;
using UnityEngine.InputSystem; // новая Input System

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Stats Source")]
    public PlayerStatsSO stats;

    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Crouch")]
    public float crouchSpeed = 2f;          // скорость при приседе
    public float crouchHeight = 1.0f;       // высота контроллера при приседе
    public float standingHeight = 1.8f;     // высота стоя


    [Header("Camera")]
    public Transform cameraTransform;       // сюда дадим Main Camera
    public float mouseSensitivity = 1f;     // чувствительность мыши
    public float maxLookAngle = 80f;        // ограничение вверх/вниз

    private CharacterController controller;
    private Vector2 moveInput;  // из Input System (WASD)
    private Vector2 lookInput;  // из Input System (мышь)
    private float verticalVelocity;
    private float currentSpeed;
    private float cameraPitch = 0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (stats != null)
        {
            // Берём скорости из ScriptableObject
            walkSpeed = stats.walkSpeed;
            sprintSpeed = stats.sprintSpeed;
            crouchSpeed = stats.crouchSpeed;
        }
        else
        {
            Debug.LogWarning("PlayerMovement: Stats SO не назначен, используются значения из инспектора.");
        }

        currentSpeed = walkSpeed;
    }

    // Эти методы будет вызывать Player Input (через Events)

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && controller.isGrounded)
        {
            // формула прыжка по физике
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            currentSpeed = sprintSpeed;
        }
        else if (context.canceled)
        {
            currentSpeed = walkSpeed;
        }
    }
    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Переход в присед
            controller.height = crouchHeight;
            currentSpeed = crouchSpeed;
        }
        else if (context.canceled)
        {
            // Возврат в полный рост
            controller.height = standingHeight;
            currentSpeed = walkSpeed;
        }
    }


    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        // движение по локальным осям игрока
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= currentSpeed;

        // гравитация
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f; // прижимаем к земле
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void HandleLook()
    {
        // горизонтальный поворот (вокруг оси Y)
        float yaw = lookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * yaw);

        // вертикальный поворот камеры (вверх/вниз)
        float pitchDelta = -lookInput.y * mouseSensitivity * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch + pitchDelta, -maxLookAngle, maxLookAngle);

        if (cameraTransform != null)
        {
            cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
        }
    }
}
