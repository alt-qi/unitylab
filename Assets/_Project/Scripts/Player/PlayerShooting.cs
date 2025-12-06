using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // для InputAction.CallbackContext

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting")]
    public float damage = 25f;       // сколько урона наносим
    public float range = 100f;       // дальность выстрела

    [Header("Effects")]
    public LineRenderer lineRenderer; // ссылка на LineRenderer
    public float lineDuration = 0.05f; // как долго показывать луч (в секундах)

    private Camera cam;


    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogWarning("PlayerShooting: Камера не найдена на объекте!");
        }

        // Если lineRenderer не задан в инспекторе — пробуем найти на этом объекте
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
        }

        // На старте луч скрыт
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }


    // Этот метод будет вызываться из Player Input (Fire)
    public void OnFire(InputAction.CallbackContext context)
    {
        // Стреляем только в момент нажатия, а не когда держим кнопку
        if (!context.performed)
            return;

        ShootRaycast();
    }

    private void ShootRaycast()
    {
        if (cam == null)
            return;

        // Направление взгляда камеры
        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Vector3 startPos = ray.origin;
        Vector3 endPos = ray.origin + ray.direction * range;

        if (Physics.Raycast(ray, out RaycastHit hitInfo, range))
        {
            endPos = hitInfo.point; // если попали — конец луча в точке попадания
            Debug.Log("Попали в: " + hitInfo.collider.name);

            Health health = hitInfo.collider.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
        }
        else
        {
            Debug.Log("Промах");
        }

        // Рисуем луч, если есть LineRenderer
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            StartCoroutine(ShowLine());
        }
    }

    private IEnumerator ShowLine()
    {
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(lineDuration);
        lineRenderer.enabled = false;
    }
}
