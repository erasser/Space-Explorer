using UnityEngine;
using static UniverseController;
using Vector3 = UnityEngine.Vector3;

public class Laser : CachedMonoBehaviour
{
    public bool autoAim;
    public float range = 500;
    [Tooltip("units: m / frame (1/60 of m/s)")]
    public float initialShootSpeed = 1.1f;
    public float shootDelay = .3f; // Po změně ve WeaponSystem nevim, jestli jsem nerozbil rychlost
    float _selfDestructAtTime;              // Consider using distance instead of lifespan
    // public Vector3 initialSpeed;
    Vector3 _speedV3;  // meters per frame
    float _sqrRaycastLength;

    void FixedUpdate()
    {
        CheckLifeSpan();

        UpdateTransform();

        CheckIntersection();
    }

    public void Setup(Vector3 position, Vector3 rotation, Vector3 speed)
    {
        _speedV3 = speed;
        _sqrRaycastLength = Mathf.Pow(_speedV3.z, 2);  // TODO !!
        _selfDestructAtTime = Time.time + range / _speedV3.z;
        transform.position = position;

        if (autoAim)
            transform.LookAt(MouseCursorHit.point);
        else
            transform.rotation = Quaternion.Euler(rotation);
    }

    void CheckLifeSpan()
    {
        if (Time.time > _selfDestructAtTime)
        {
            Destroy(gameObject);
        }
    }

    void UpdateTransform()
    {
        transform.Translate(_speedV3);
        // transform.LookAt(universeController.mainCamera.transform);
    }

    void CheckIntersection()
    {
        // Debug.DrawRay(transform.position, transform.forward * speedV3.z, Color.magenta);

        if (Physics.Raycast(transform.position, transform.forward, out var hit, _sqrRaycastLength, universeController.shootable))
        {
            universeController.LaunchHitEffect(hit.point, hit.normal);

            hit.collider.gameObject.GetComponent<Damageable>()?.TakeDamage(10);

            Destroy(gameObject);
        }
    }

    // void RotateWeaponSlot()
    // {
    //     if (autoAim)
    //         transformCached.LookAt(MouseCursorHit.point);
    // }
}
