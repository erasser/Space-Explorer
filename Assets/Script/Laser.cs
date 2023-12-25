using UnityEngine;
using static UniverseController;
using Vector3 = UnityEngine.Vector3;

public class Laser : CachedMonoBehaviour
{
    public bool autoAim;
    [Tooltip("m")]
    public float range = 500;
    [Tooltip("m/s")]
    public float initialShootSpeed = 1.1f;
    public float shootDelay = .3f;
    float _selfDestructAtTime;
    Vector3 _speedV3;  // m / s
    float _sqrRaycastLength;

    void FixedUpdate()
    {
        CheckLifeSpan();

        UpdateTransform();

        CheckIntersection();
    }

    public void Setup(Vector3 position, float rotationY, Vector3 speed)
    {
        _speedV3 = speed;
        _sqrRaycastLength = speed.z;  // TODO: Myslet na slow-motion, mělo by obsahovat Time.fixedDeltaTime a po přechodu do slow-mo updatovat - to se asi týká jen už vystřelených projektilů  
        _selfDestructAtTime = Time.time + range / (_speedV3.z / Time.fixedDeltaTime);
        transform.position = position;

        if (autoAim)
            transform.LookAt(MouseCursorHit.point);
        else
            transform.rotation = Quaternion.Euler(new(0, rotationY, 0));  // TODO: Nedalo by se to nějak zjednodušit? :D
    }

    void CheckLifeSpan()
    {
        if (Time.time > _selfDestructAtTime)
            Destroy(gameObject);
    }

    void UpdateTransform()
    {
        transform.Translate(_speedV3);
    }

    void CheckIntersection()
    {
        // Debug.DrawRay(transform.position, transformCached.forward * _speedV3.z, Color.yellow);

        // if (Physics.Raycast(transformCached.position, transformCached.forward, out var hit, _sqrRaycastLength, universeController.shootableLayer))
        if (Physics.Raycast(new(transformCached.position.x, 0, transformCached.position.z), transformCached.forward, out var hit, _sqrRaycastLength, universeController.shootableLayer))
        {
            universeController.LaunchHitEffect(hit.point, hit.normal);

            hit.collider.gameObject.GetComponent<Damageable>()?.TakeDamage(10);

            Destroy(gameObject);
        }
    }
}
