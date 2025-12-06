using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private PlayerStatsSO stats;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    private float standingHeight;
    private Vector3 standingCenter;
    private bool isCrouching;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float maxLookAngle = 85f;

    [Header("Aim (ADS)")]
    [SerializeField] private float aimFov = 40f;
    [SerializeField] private Vector3 aimCameraOffset = new Vector3(0.1f, -0.05f, 0.1f);
    [SerializeField] private float aimSensitivityMultiplier = 0.5f;
    [SerializeField] private float fovLerpSpeed = 10f;
    [SerializeField] private float cameraPosLerpSpeed = 10f;

    [Header("Sprint Camera Bob")]
    [SerializeField] private float sprintFov = 70f;
    [SerializeField] private float cameraBobAmplitude = 0.05f;
    [SerializeField] private float cameraBobFrequency = 10f;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float verticalVelocity;
    private bool isSprinting;
    private bool isAiming;

    private float cameraPitch;
    private Camera playerCamera;
    private float defaultFov;
    private Vector3 defaultCameraLocalPos;
    private Vector3 aimCameraLocalPos;
    private float bobTimer;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (stats != null)
        {
            walkSpeed = stats.walkSpeed;
            sprintSpeed = stats.sprintSpeed;
            crouchSpeed = stats.crouchSpeed;
        }

        standingHeight = controller.height;
        standingCenter = controller.center;

        if (cameraTransform != null)
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
            defaultCameraLocalPos = cameraTransform.localPosition;
            aimCameraLocalPos = defaultCameraLocalPos + aimCameraOffset;

            if (playerCamera != null)
                defaultFov = playerCamera.fieldOfView;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleLook();
        HandleCameraEffects();
    }

    #region Input callbacks (New Input System)

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
        if (!context.performed)
            return;

        if (controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
            isSprinting = true;
        else if (context.canceled)
            isSprinting = false;
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        ToggleCrouch();
    }

    // НОВЫЙ input-колбэк для прицеливания (Aim)
    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.performed || context.started)
            isAiming = true;
        else if (context.canceled)
            isAiming = false;
    }

    #endregion

    private void HandleMovement()
    {
        // базовая горизонтальная скорость
        float currentSpeed;
        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isSprinting && !isAiming) // прицеливание отключает спринт
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= currentSpeed;

        // гравитация
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f; // маленький прижим к земле
        }
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (cameraTransform == null)
            return;

        float sensitivity = mouseSensitivity * (isAiming ? aimSensitivityMultiplier : 1f);

        float yaw = lookInput.x * sensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * yaw);

        float pitchDelta = -lookInput.y * sensitivity * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch + pitchDelta, -maxLookAngle, maxLookAngle);

        cameraTransform.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);
    }

    private void HandleCameraEffects()
    {
        if (cameraTransform == null || playerCamera == null)
            return;

        // 1) целевой FOV
        float targetFov = defaultFov;

        if (isAiming)
        {
            targetFov = aimFov;
        }
        else if (isSprinting && controller.isGrounded && moveInput.magnitude > 0.1f)
        {
            targetFov = sprintFov;
        }

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFov,
            fovLerpSpeed * Time.deltaTime
        );

        // 2) целевая позиция камеры (ADS-офсет)
        Vector3 targetLocalPos = isAiming ? aimCameraLocalPos : defaultCameraLocalPos;

        // 3) bob при беге
        if (isSprinting && controller.isGrounded && moveInput.magnitude > 0.1f && !isAiming)
        {
            bobTimer += Time.deltaTime * cameraBobFrequency;
            float bobOffsetY = Mathf.Sin(bobTimer) * cameraBobAmplitude;
            Vector3 bob = new Vector3(0f, bobOffsetY, 0f);

            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                targetLocalPos + bob,
                cameraPosLerpSpeed * Time.deltaTime
            );
        }
        else
        {
            bobTimer = 0f;
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                targetLocalPos,
                cameraPosLerpSpeed * Time.deltaTime
            );
        }
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;

        if (isCrouching)
        {
            controller.height = crouchHeight;
            controller.center = new Vector3(standingCenter.x, crouchHeight / 2f, standingCenter.z);
        }
        else
        {
            controller.height = standingHeight;
            controller.center = standingCenter;
        }
    }
}
