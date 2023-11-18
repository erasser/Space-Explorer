using UnityEngine;

public class Damageable : MonoBehaviour
{
    public float armor = 100;
    public float currentArmor;
    public float shield = 100;
    public float currentShield;

    public void TakeDamage(float damage)
    {
        // _lastDamagedTime = Time.time;

        if (currentShield > 0)
        {
            currentShield -= damage; // Apply damage to shield
            // UpdateShieldBar();

            if (currentShield < 0)
            {
                currentArmor += currentShield; // Apply damage to armor, if damage exceeds the shield level (this actually subtracts)
                currentShield = 0;
                // UpdateArmorBar();
            }
        }
        else
        {
            currentArmor -= damage;
            // UpdateArmorBar();
        }

        if (currentArmor <= 0)
            Destroy(gameObject);
    }
}
