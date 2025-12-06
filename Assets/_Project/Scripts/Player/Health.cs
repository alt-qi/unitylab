using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Stats Source")]
    public PlayerStatsSO stats;   // <-- Ссылка на ScriptableObject

    [Header("Runtime Values")]
    public float currentHealth;
    public float currentArmor;

    // События для UI
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float, float> OnArmorChanged;  // current, max
    public event Action OnDied;                        // смерть объекта


    private void Awake()
    {
        if (stats == null)
        {
            Debug.LogError("Health: ScriptableObject Stats не назначен!");
            return;
        }

        // Читаем данные из SO
        currentHealth = stats.maxHealth;
        currentArmor = stats.maxArmor;

        // уведомляем UI о первоначальном состоянии
        OnHealthChanged?.Invoke(currentHealth, stats.maxHealth);
        OnArmorChanged?.Invoke(currentArmor, stats.maxArmor);
    }


    public void TakeDamage(float damage)
    {
        float remainingDamage = damage;

        // Сначала урон уходит в броню
        if (currentArmor > 0f)
        {
            float armorDamage = Mathf.Min(currentArmor, remainingDamage);
            currentArmor -= armorDamage;
            remainingDamage -= armorDamage;
        }

        // Если после брони остался урон — он идёт в здоровье
        if (remainingDamage > 0f)
        {
            currentHealth -= remainingDamage;

            if (currentHealth < 0f)
                currentHealth = 0f;
        }

        Debug.Log($"{gameObject.name} получил урон {damage}. HP: {currentHealth}, Armor: {currentArmor}");

        // уведомляем UI
        OnHealthChanged?.Invoke(currentHealth, stats.maxHealth);
        OnArmorChanged?.Invoke(currentArmor, stats.maxArmor);
        
        // Проверяем смерть
        if (currentHealth == 0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;

        // Ограничиваем максимумом из SO
        if (currentHealth > stats.maxHealth)
            currentHealth = stats.maxHealth;
        
        // уведомляем UI
        OnHealthChanged?.Invoke(currentHealth, stats.maxHealth);

        Debug.Log($"{gameObject.name} восстановил {amount} HP. Текущее здоровье: {currentHealth}");
    }


    // метод смерти — пока просто лог и можем выключить объект
    private void Die()
    {
        Debug.Log($"{gameObject.name} умер");

        OnDied?.Invoke();

        // временно просто выключаем объект
        // позже можно будет сделать респавн, анимацию, и т.п.
        gameObject.SetActive(false);
    }
}
