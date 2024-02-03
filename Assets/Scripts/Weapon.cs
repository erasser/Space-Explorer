using UnityEngine;
using static Ship;

// This is a weapon socket

public class Weapon : CachedMonoBehaviour
{
    public GameObject projectilePrefab;
    // public float initialShootSpeed = 100f;
    public float shootDelay = .3f;
    Vector3 _initialShootSpeedV3;           // to get Vector3 from float
    // static Vector3 _actualProjectileSpeed;  // for raycast length calculation
    Vector3 _shootVectorCoefficient;        // for performance optimization
    float _lastShootTime;
    Ship _ship;
    // public event Action<Vector3, float, Vector3> WeaponFire;

    void Start()
    {
        // _initialShootSpeedV3 = new (0, 0, projectilePrefab.GetComponent<Projectile>().speed * Time.fixedDeltaTime);
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
        if (!_ship.isFiring || Time.time - _lastShootTime < shootDelay)
            return;

        _lastShootTime = Time.time;

        var newProjectile = Instantiate(projectilePrefab);
        // newLaser.GetComponent<Laser>().Setup(
        //     transformCached.position,
        //     new(0, transformCached.eulerAngles.y, 0),  // TODO: stačilo by předávat jen float
        //     _ship.GetForwardSpeed() * _shootVectorCoefficient + _initialShootSpeedV3);  // TODO: stačilo by předávat jen float

        /*Laser componentLaser = newProjectile.GetComponent<Laser>();
        if (componentLaser)
        {
            componentLaser.weapon = this;
            componentLaser.Setup(
                transformCached.position,
                transformCached.eulerAngles.y/*,
                // _ship.GetForwardSpeed() * _shootVectorCoefficient + _initialShootSpeedV3);  // TODO: stačilo by předávat jen float
                _initialShootSpeedV3#1#);
            return;
        }*/

        var projectileComponent = newProjectile.GetComponent<Projectile>();
        projectileComponent.Setup(transformCached.position, transformCached.eulerAngles.y);

        Rocket componentRocket = newProjectile.GetComponent<Rocket>();
        if (componentRocket)
        {
            if (_ship == ActiveShip)
            {
                componentRocket.SetTarget(_ship.GetClosestShipInRange(EnemyShips).ship);
            }
            else
                componentRocket.SetTarget(ActiveShip);
        }
    }
}