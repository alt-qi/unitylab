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
    [SerializeField] private float crouchCameraYOffset = -0.5f;
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

    [Header("Recoil")]
    [SerializeField] private float recoilReturnSpeed = 8f; // чем выше, тем быстрее камера возвращается
    private float recoilOffset; // дополнительный угол по X от отдачи

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

    #region Input callbacks

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

    // Aim (ПКМ)
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
        float currentSpeed;
        if (isCrouching)
            currentSpeed = crouchSpeed;
        else if (isSprinting && !isAiming) // прицеливаешься — не спринтишь
            currentSpeed = sprintSpeed;
        else
            currentSpeed = walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move *= currentSpeed;

        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }
        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }

    private void HandleLook()
    {
        if (cameraTransform == null)
            return;

        // сглаживаем отдачу → стремится к 0
        recoilOffset = Mathf.Lerp(recoilOffset, 0f, recoilReturnSpeed * Time.deltaTime);

        float sensitivity = mouseSensitivity * (isAiming ? aimSensitivityMultiplier : 1f);

        float yaw = lookInput.x * sensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * yaw);

        float pitchDelta = -lookInput.y * sensitivity * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch + pitchDelta, -maxLookAngle, maxLookAngle);

        float totalPitch = Mathf.Clamp(cameraPitch + recoilOffset, -maxLookAngle, maxLookAngle);
        cameraTransform.localEulerAngles = new Vector3(totalPitch, 0f, 0f);
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

        Vector3 baseLocalPos = defaultCameraLocalPos;

        // если сидим — опускаем камеру
        if (isCrouching)
        {
            baseLocalPos += new Vector3(0f, crouchCameraYOffset, 0f);
        }

        // если целимся — добавляем оффсет прицеливания поверх текущего базового положения
        Vector3 targetLocalPos = isAiming ? baseLocalPos + aimCameraOffset : baseLocalPos;

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
        // хотим встать
        if (isCrouching)
        {
            if (!CanStandUp())
                return; // над головой потолок, сидим дальше

            isCrouching = false;
            controller.height = standingHeight;
            controller.center = standingCenter;
        }
        else // хотим присесть
        {
            isCrouching = true;
            controller.height = crouchHeight;
            controller.center = new Vector3(standingCenter.x, crouchHeight / 2f, standingCenter.z);
        }
    }

    // проверка, есть ли место, чтобы выпрямиться
    private bool CanStandUp()
    {
        // точка старта — примерно центр коллизии в приседе
        Vector3 origin = transform.position + Vector3.up * (crouchHeight / 2f);
        float checkDistance = standingHeight - crouchHeight + 0.1f;
        float radius = controller.radius * 0.9f;

        // если сверху что-то есть → стоять нельзя
        return !Physics.SphereCast(origin, radius, Vector3.up, out _, checkDistance, ~0, QueryTriggerInteraction.Ignore);
    }

    // вызывается стрельбой, чтобы добавить небольшой "кик" камеры
    public void AddRecoil(float recoilAmount)
    {
        // отрицательное значение двигает камеру вверх (как отдача)
        recoilOffset -= recoilAmount;
    }
}
