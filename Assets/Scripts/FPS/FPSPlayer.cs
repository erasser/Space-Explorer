using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSPlayer : MonoBehaviour
{
    public static FPSPlayer fpsPlayer;
    public GameObject flashLight;
    [HideInInspector]
    public bool isShooting;
    readonly List<FPSWeapon> _weapons = new(); 
    FPSWeapon _activeWeapon;
    Coroutine _shootCoroutine;
    float _lastShootTime;

    void Start()
    {
        fpsPlayer = this;

        foreach (Transform tr in transform.Find("Joint/PlayerCamera/weapons"))
            _weapons.Add(tr.GetComponent<FPSWeapon>());

        _activeWeapon = _weapons[0];
    }

    void Update()
    {
        ProcessShooting();
    }

    void ProcessShooting()
    {
        if (!isShooting)
            return;
        
        if (Time.time > _lastShootTime + _activeWeapon.projectilePrefab.delay)
        {
            _activeWeapon.Shoot();
            _lastShootTime = Time.time;
        }
    }
}
