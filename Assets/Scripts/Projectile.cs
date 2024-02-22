using UnityEngine;
using static UniverseController;
using static Ship;

public abstract class Projectile : CachedMonoBehaviour
{
    public bool autoAim;
    [Tooltip("m")]
    public float range = 500;
    [Tooltip("m/s")]
    public float speed = 200;
    float _sqrRaycastLength;
    float _selfDestructAtTime;
    [HideInInspector]
    public Vector3 velocity;  // m / s
    LayerMask _shootableLayerMask;
    // public bool followTarget;
    // [Range(0, 200)]
    // public float followTargetRotationSpeed = 50;

    // new void Awake()
    // {
    //     base.Awake();
    // }
    
    public void Start()
    {
        velocity = speed * Time.fixedDeltaTime * Vector3.forward;
    }

    public void FixedUpdate()
    {
        CheckLifeSpan();

        CheckIntersection();

        UpdatePosition();

        // UpdateRotation();
    }

    public void Setup(Vector3 position, float rotationY, LayerMask shootableLayerMask, Ship originShip  /*, Vector3 speed*/)
    {
        // _speedV3 = speed;
        _sqrRaycastLength = speed * Time.fixedDeltaTime;  // TODO: Myslet na slow-motion, mělo by obsahovat Time.fixedDeltaTime a po přechodu do slow-mo updatovat - to se asi týká jen už vystřelených projektilů  
        _selfDestructAtTime = Time.time + range / (speed/* / Time.fixedDeltaTime*/);
        transform.position = position;
        _shootableLayerMask = shootableLayerMask;

        if (autoAim)
        {
            if (originShip.IsPlayer())
                transform.LookAt(MouseCursorHit.point);
            else
                transform.LookAt(ActiveShip.transformCached);
        }
        else
            transform.rotation = Quaternion.Euler(new(0, rotationY, 0));  // TODO: Nedalo by se to nějak zjednodušit? :D
    }

    void CheckIntersection()
    {
        // Debug.DrawRay(transform.position, transformCached.forward * _speedV3.z, Color.yellow);

        // if (Physics.Raycast(transformCached.position, transformCached.forward, out var hit, _sqrRaycastLength, universeController.shootableLayer))
        if (Physics.Raycast(new(transform.position.x, 0, transform.position.z), transform.forward, out var hit, _sqrRaycastLength, _shootableLayerMask))
        {
            Uc.LaunchHitEffect(hit.point, hit.normal);

            hit.collider.gameObject.GetComponent<Damageable>()?.TakeDamage(10);
            // TODO: SEND MESSAGE
            // LaserHitEvent(this);

            Destroy(gameObject);
        }
    }

    void CheckLifeSpan()
    {
        if (Time.time > _selfDestructAtTime)
            Destroy(gameObject);
    }

    void UpdatePosition()
    {
        // print(name + " velocity = " + velocity);
        // transform.Translate(Vector3.forward * 2);
        transform.Translate(velocity);
        // transform.position = new(Random.Range(-50, 50), 0 , 0);
    }

    /*void UpdateRotation()
    {
        if (!followTarget)
            return;

        var toTargetVector = (_target.position - _rb.transform.position).normalized;
        var cross = Vector3.Cross(toTargetVector, transform.forward);
        _rb.AddTorque(- 2 * cross.magnitude * cross.normalized, ForceMode.Force);  // Force is proportional to the angle
    }*/

}
