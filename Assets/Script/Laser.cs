using UnityEngine;
using UnityEngine.VFX;
using static UniverseController;
using Vector3 = UnityEngine.Vector3;

public class Laser : MonoBehaviour
{
    // public VisualEffect hitEffect;  // TODO
    static readonly float _lifeSpan = 15;
    float _selfDestructAtTime;              // Consider using distance instead of lifespan
    // public Vector3 initialSpeed;
    [HideInInspector]
    public Vector3 speedV3;  // meters per frame
    [HideInInspector]
    public float sqrRaycastLength;

    void Start()
    {
        _selfDestructAtTime = Time.time + _lifeSpan;
    }

    void FixedUpdate()
    {
        CheckLifeSpan();

        UpdatePosition();

        CheckIntersection();
    }

    void CheckLifeSpan()
    {
        if (Time.time > _selfDestructAtTime)
        {
            Destroy(gameObject);
        }
    }

    void UpdatePosition()
    {
        transform.Translate(speedV3);
    }

    void CheckIntersection()
    {
        // Debug.DrawRay(transform.position, transform.forward * speedV3.z, Color.magenta);

        if (Physics.Raycast(transform.position, transform.forward, out var hit, Mathf.Infinity, universeController.damageable))
        {
            if ((hit.point - transform.position).sqrMagnitude > sqrRaycastLength)
                return;

            Destroy(gameObject);
            LaunchHitEffect(hit.point, hit.normal);
            hit.collider.gameObject.GetComponent<Damageable>()?.TakeDamage(10);
        }
    }

    void LaunchHitEffect(Vector3 point, Vector3 normal)
    {
        // var newHitEffect = Instantiate(hitEffect);
        //
        // newHitEffect.transform.position = point;
        // newHitEffect.transform.rotation = Quaternion.LookRotation(normal);
        // newHitEffect.Play();
    }
}
