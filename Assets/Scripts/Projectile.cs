using UnityEngine;
using UnityEngine.Serialization;
using static UniverseController;
using static Ship;

public abstract class Projectile : MonoBehaviour
{
    public AutoAim autoAimType;
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

    public enum AutoAim
    {
        None,
        PositionAutoAim,
        PositionPredictionAutoAim
    }
    
    public void Start()
    {
        velocity = speed * Time.fixedDeltaTime * Vector3.forward;
    }

    public void FixedUpdate()
    {
        CheckLifeSpan();

        CheckIntersection();
    }

    public void Setup(Ship originShip, Vector3 position, float rotationY)
    {
        // _speedV3 = speed;
        _sqrRaycastLength = speed * Time.fixedDeltaTime;  // TODO: Myslet na slow-motion, mělo by obsahovat Time.fixedDeltaTime a po přechodu do slow-mo updatovat - to se asi týká jen už vystřelených projektilů  
        _selfDestructAtTime = Time.time + range / (speed/* / Time.fixedDeltaTime*/);
        transform.position = position;
        _shootableLayerMask = originShip.shootableLayerMasks;
        Rocket componentRocket = GetComponent<Rocket>();

        if (originShip.IsPlayer())
        {
            if (autoAimType == AutoAim.None)
            {
                transform.rotation = Quaternion.Euler(new(0, rotationY, 0));

                if (componentRocket)
                    componentRocket.SetTarget(null);
            }
            else
            {
                transform.LookAt(MouseCursorHit.point);

                if (componentRocket)
                {
                    var targetShip = originShip.GetClosestShipInRange(EnemyShips).ship;
                    componentRocket.SetTarget(targetShip);
                }
            }
        }
        else  // enemy shooting
        {
            if (autoAimType == AutoAim.None)
                transform.rotation = Quaternion.Euler(new(0, rotationY, 0));    // TODO: Nedalo by se to nějak zjednodušit? :D
            else if (autoAimType == AutoAim.PositionAutoAim)
                transform.LookAt(ActiveShip.transform);
            else
                transform.LookAt(ActiveShip.transform.position + GetPredictedPositionOffset(ActiveShip, ActiveShip.velocityEstimator.GetVelocityEstimate(), originShip.gameObject, speed));

            if (componentRocket)
                componentRocket.SetTarget(ActiveShip);
        }
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

    void DestroyMe()
    {
        Destroy(gameObject);
    }
}
