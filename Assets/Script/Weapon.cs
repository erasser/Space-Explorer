using UnityEngine;
using static Ship;
// This is on weapon socket

public class Weapon : CachedMonoBehaviour
{
    public Laser projectilePrefab;
    public float shootDelay = .3f; // Po změně ve WeaponSystem nevim, jestli jsem nerozbil rychlost. TODO: Ten targetHelper bude muset být v Update()
    [Tooltip("units: m / frame (1/60 of m/s)")]
    public float initialShootSpeed = 1.1f;  // config
    Vector3 _initialShootSpeedV3;           // to get Vector3 from float
    // static Vector3 _actualProjectileSpeed;  // for raycast length calculation
    Vector3 _shootVectorCoefficient;        // for performance optimization
    // public bool isActive;
    // readonly List<Transform> _shootPositions = new ();
    float _lastShootTime;
    // [HideInInspector]
    // public Vector3 relativePosition;

    void Start()
    {
        _initialShootSpeedV3 = new (0, 0, initialShootSpeed);
        _shootVectorCoefficient = Time.fixedDeltaTime * Vector3.forward;  // fixedDeltaTime is here to convert m/second to m/frame
    }

    void Update()
    {
        AutoFire();
    }

    void AutoFire()
    {
        if (!ActiveShip.isFiring || Time.time - _lastShootTime < shootDelay)
            return;

        _lastShootTime = Time.time;

        var newLaser = Instantiate(projectilePrefab, transformCached.position, Quaternion.Euler(0, transformCached.eulerAngles.y, 0));
        Laser newLaserComponent = newLaser.GetComponent<Laser>();
        newLaserComponent.speedV3 = ActiveShip.forwardSpeed * _shootVectorCoefficient + _initialShootSpeedV3;
        newLaserComponent.sqrRaycastLength = Mathf.Pow(newLaserComponent.speedV3.z, 2);
            
        // TODO: Zkontrolovat, že to správně castí
    }
    
    /*void AutoFire()
    {
        if (!isActive || Time.time - _lastShootTime < shootDelay)
            return;

        _lastShootTime = Time.time;

        foreach (Transform startPosTransform in _shootPositions)  //**#1#/ TODO: Je potřeba vzít globální pozici, projectilePositions budou muset být GameObjecty.
        {
            var newLaser = Instantiate(projectilePrefab, startPosTransform.position, transform.rotation);
            Laser newLaserComponent = newLaser.GetComponent<Laser>();
            newLaserComponent.speedV3 = ActiveShip.forwardSpeed * _shootVectorCoefficient + _initialShootSpeedV3;
            newLaserComponent.sqrRaycastLength = Mathf.Pow(newLaserComponent.speedV3.z, 2);
        }
    }*/
}