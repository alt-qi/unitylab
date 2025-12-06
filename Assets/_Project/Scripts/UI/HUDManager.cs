using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private IconBar healthBar;
    [SerializeField] private IconBar armorBar;
    
    [Header("Target")]
    [SerializeField] private Health playerHealth;

    private void OnEnable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += UpdateHealthUI;
            playerHealth.OnArmorChanged += UpdateArmorUI;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= UpdateHealthUI;
            playerHealth.OnArmorChanged -= UpdateArmorUI;
        }
    }

    private void Start()
    {
        if (playerHealth != null && playerHealth.stats != null)
        {
            UpdateHealthUI(playerHealth.currentHealth, playerHealth.stats.maxHealth);
            UpdateArmorUI(playerHealth.currentArmor, playerHealth.stats.maxArmor);
        }
    }
    
    private void UpdateHealthUI(float current, float max)
    {
        float percent = (max > 0) ? current / max : 0f;
        healthBar.UpdateBar(percent);
    }
    
    private void UpdateArmorUI(float current, float max)
    {
        float percent = (max > 0) ? current / max : 0f;
        armorBar.UpdateBar(percent);
    }
}