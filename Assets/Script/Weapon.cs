using UnityEngine;
using static Ship;
// This is on weapon socket

public class Weapon : CachedMonoBehaviour
{
    public Laser projectilePrefab;
    Vector3 _initialShootSpeedV3;           // to get Vector3 from float
    // static Vector3 _actualProjectileSpeed;  // for raycast length calculation
    Vector3 _shootVectorCoefficient;        // for performance optimization
    float _lastShootTime;
    Ship _ship;

    void Start()
    {
        _initialShootSpeedV3 = new (0, 0, projectilePrefab.initialShootSpeed * Time.fixedDeltaTime);
        _ship = transform.root.gameObject.GetComponent<Ship>();
        _shootVectorCoefficient = Time.fixedDeltaTime * Vector3.forward;  // fixedDeltaTime is here to convert m/second to m/frame
        // TODO: ↑ Bude potřeba vyzkoušet ve slow motion
    }

    void Update()
    {
        AutoFire();
    }

    void AutoFire()
    {
        if (!_ship.isFiring || Time.time - _lastShootTime < projectilePrefab.shootDelay)
            return;

        _lastShootTime = Time.time;

        var newLaser = Instantiate(projectilePrefab);
        newLaser.GetComponent<Laser>().Setup(
            transformCached.position,
            new(0, transformCached.eulerAngles.y, 0),
            _ship.GetForwardSpeed() * _shootVectorCoefficient + _initialShootSpeedV3);
    }
}