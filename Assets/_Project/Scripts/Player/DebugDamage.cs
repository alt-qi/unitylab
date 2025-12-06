using UnityEngine;
using UnityEngine.InputSystem; // новая Input System

public class DebugDamage : MonoBehaviour
{
    public Health targetHealth;       // кого бьём/лечим
    public float damageAmount = 20f;  // сколько урона наносим
    public float healAmount = 15f;    // сколько лечим

    private void Update()
    {
        if (targetHealth == null)
        {
            return;
        }

        // Урон по клавише H
        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            targetHealth.TakeDamage(damageAmount);
        }

        // Лечение по клавише J
        if (Keyboard.current != null && Keyboard.current.jKey.wasPressedThisFrame)
        {
            targetHealth.Heal(healAmount);
        }
    }
}
