using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Stats/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Player Stats")]
    public float maxHealth = 100f;
    public float maxArmor = 50f;
    public float baseDamage = 25f;

    public float walkSpeed = 4f;
    public float sprintSpeed = 7f;
    public  float crouchSpeed = 2f;
}
