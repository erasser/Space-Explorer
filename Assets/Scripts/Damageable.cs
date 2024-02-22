using System;
using UnityEngine;

public class Damageable : MonoBehaviour
{
    public float maxArmor = 100;
    [HideInInspector]
    public float currentArmor;
    public float maxShield = 0;
    [HideInInspector]
    public float currentShield;

    void Start()
    {
        // Laser.LaserHitEvent += OnLaserHit(Laser.LaserHitDelegate l);
        currentArmor = maxArmor;
        currentShield = maxShield;
    }

    // void OnLaserHit()
    // {
    //     
    // }

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
