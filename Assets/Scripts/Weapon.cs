using UnityEngine;

// This is a weapon socket

public class Weapon : MonoBehaviour
{
    public GameObject projectilePrefab;
    // public float initialShootSpeed = 100f;
    public float shootInterval = .3f;
    [Tooltip("Used for alternating fire, e.g. left ↔ right.\nSet to 0 for no alternating.\nSet to Shoot 'interval / 2' for regular double alternating.")]
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
        if (!_ship.isFiring || Time.time - _lastShootTime < shootInterval)
            return;

        _lastShootTime = Time.time;

        Instantiate(projectilePrefab).GetComponent<Projectile>().Setup(_ship, transform.position, transform.eulerAngles.y);
    }
}