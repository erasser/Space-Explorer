using UnityEngine;
using static UniverseController;
using Vector3 = UnityEngine.Vector3;

public class Laser : MonoBehaviour
{
    static readonly float _lifeSpan = 15;
    float _selfDestructAtTime;              // Consider using distance instead of lifespan
    // public Vector3 initialSpeed;
    [HideInInspector]
    public Vector3 speedV3;  // meters per frame
    [HideInInspector]
    public float sqrRaycastLength;
    public GameObject tmp;

    void Start()
    {
        _selfDestructAtTime = Time.time + _lifeSpan;
        tmp = GameObject.Find("Cube (1)");
    }
    
    void FixedUpdate()
    {
        CheckLifeSpan();

        UpdateTransform();

        CheckIntersection();
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
        transform.Translate(speedV3);
        // transform.LookAt(universeController.mainCamera.transform);
    }

    void CheckIntersection()
    {
        // Debug.DrawRay(transform.position, transform.forward * speedV3.z, Color.magenta);

        if (Physics.Raycast(transform.position, transform.forward, out var hit, Mathf.Infinity, universeController.shootable))
        {
            if ((hit.point - transform.position).sqrMagnitude > sqrRaycastLength)
                return;

            universeController.LaunchHitEffect(hit.point, hit.normal);

            hit.collider.gameObject.GetComponent<Damageable>()?.TakeDamage(10);

            Destroy(gameObject);
        }
    }
}
