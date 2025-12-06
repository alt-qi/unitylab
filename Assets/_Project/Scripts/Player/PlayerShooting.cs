using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private PlayerMovement playerMovement; // чтобы дернуть отдачу

    [Header("Shooting")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float lineDuration = 0.05f;

    [Header("Recoil")]
    [SerializeField] private float recoilPerShot = 2f; // 1–3 градусов обычно достаточно

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
            lineRenderer.positionCount = 2;
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        ShootRaycast();
    }

    private void ShootRaycast()
    {
        if (playerCamera == null)
            return;

        Vector3 startPos = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        Vector3 endPos;

        Ray ray = new Ray(startPos, direction);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, range))
        {
            endPos = hitInfo.point;

            // временно прям по Health, позже замените на IDamageable
            Health health = hitInfo.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
        else
        {
            endPos = startPos + direction * range;
        }

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            StopAllCoroutines();
            StartCoroutine(ShowLine());
        }

        // отдача камеры
        if (playerMovement != null)
        {
            playerMovement.AddRecoil(recoilPerShot);
        }
    }

    private IEnumerator ShowLine()
    {
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(lineDuration);
        lineRenderer.enabled = false;
    }
}
