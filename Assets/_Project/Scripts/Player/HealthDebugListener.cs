using UnityEngine;

public class HealthDebugListener : MonoBehaviour
{
    public Health target; // на кого подписываемся

    private void OnEnable()
    {
        if (target != null)
        {
            target.OnHealthChanged += HandleHealthChanged;
            target.OnArmorChanged += HandleArmorChanged;
            target.OnDied += HandleDied;
        }
    }

    private void OnDisable()
    {
        if (target != null)
        {
            target.OnHealthChanged -= HandleHealthChanged;
            target.OnArmorChanged -= HandleArmorChanged;
            target.OnDied -= HandleDied;
        }
    }

    private void HandleHealthChanged(float current, float max)
    {
        Debug.Log($"[Listener] HP: {current}/{max}");
    }

    private void HandleArmorChanged(float current, float max)
    {
        Debug.Log($"[Listener] Armor: {current}/{max}");
    }

    private void HandleDied()
    {
        Debug.Log("[Listener] Объект умер (OnDied)");
    }
}
