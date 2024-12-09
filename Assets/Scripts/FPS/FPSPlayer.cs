using System.Collections.Generic;
using UnityEngine;
using static WorldController;

public class FPSPlayer : MonoBehaviour
{
    public static FPSPlayer fpsPlayer;
    public static Transform fpsPlayerTransform;
    public GameObject flashLight;
    [HideInInspector]
    public bool isShooting;
    readonly List<FPSWeapon> _weapons = new(); 
    FPSWeapon _activeWeapon;
    Transform _activeWeaponTransform;
    Coroutine _shootCoroutine;
    float _lastShootTime;
    [Tooltip("It's meant to be targeted by enemies")]
    public Transform targetTransform;
    public float weaponOrientationSpeed = 10;

    void Awake()
    {
        fpsPlayer = this;
        fpsPlayerTransform = fpsPlayer.transform;
    }

    void Start()
    {
        foreach (Transform tr in transform.Find("Joint/PlayerCamera/weapons"))
            _weapons.Add(tr.GetComponent<FPSWeapon>());

        SetActiveWeapon(0);
    }

    void Update()
    {
        UpdateWeaponOrientation();

        ProcessShooting();
    }

    void SetActiveWeapon(int index)
    {
        _activeWeapon = _weapons[index];
        _activeWeaponTransform = _activeWeapon.transform;
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

    void UpdateWeaponOrientation()
    {
        var pointInWorld = FPSCamera.ScreenToWorldPoint(ScreenHalfResolution);

        if (Physics.Raycast(pointInWorld, FPSCamera.transform.forward, out var hit))
        {
            var startVector = _activeWeapon.transform.forward;
            var endVector = hit.point - _activeWeapon.transform.position;
            var resultVector = Vector3.RotateTowards(startVector, endVector, Time.deltaTime * weaponOrientationSpeed, 0);
             _activeWeapon.transform.LookAt(_activeWeapon.transform.position + resultVector);
        }
        else
            _activeWeapon.transform.localRotation = Quaternion.identity;
    }
}
