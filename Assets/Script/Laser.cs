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

    public void Setup(Vector3 position, Vector3 rotation, Vector3 speed)
    {
        _speedV3 = speed;
        _sqrRaycastLength = Mathf.Pow(_speedV3.z, 2);  // TODO !! (Myslet na slow-motion, mělo by obsahovat Time.fixedDeltaTime a po přechodu do slow-mo updatovat)  
        _selfDestructAtTime = Time.time + range / (_speedV3.z / Time.fixedDeltaTime);
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
        transform.Translate(Time.fixedDeltaTime * 100 * _speedV3);  // TODO: Z (Time.fixedDeltaTime * 100) udělat proměnnou měnící se po přechodu do slow-motion
        // transform.LookAt(universeController.mainCamera.transform);
    }

    void CheckIntersection()
    {
        // Debug.DrawRay(transform.position, transform.forward * speedV3.z, Color.magenta);

        if (Physics.Raycast(transform.position, transform.forward, out var hit, _sqrRaycastLength, universeController.shootableLayer))
        {
            universeController.LaunchHitEffect(hit.point, hit.normal);

            hit.collider.gameObject.GetComponent<Damageable>()?.TakeDamage(10);

            Destroy(gameObject);
        }
    }
}
