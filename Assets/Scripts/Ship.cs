using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;
using static UniverseController;

// Ship requirements: This script, "ship" layer (to můžu asi udělat dynamicky)

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Ship : MonoBehaviour
{
    public float speed = 10;
    public float rotationSpeed = 3;
    Vector3 _rotationSpeedVector;
    [Range(0, 90)]
    public float maxRollAngle = 80;
    [Tooltip("Jet to ship angle in degrees.\nIt affects individual jet activation condition.\nHigher value => more jets\n\nDO NOT change in playmode!")]
    [Range(0, 90)]      // -.5 odpovídá 60 °, čili setupu tří jetů do hvězdy
    public float jetAngle = 45;
    // public static readonly List<Ship> ShipList = new();
    public static readonly List<Ship> EnemyShips = new();
    [HideInInspector]
    public Vector3 moveVector;
    float _jetAngleCos;
    Vector3 _customTarget;
    [HideInInspector]
    public Rigidbody rb;
    Transform _rbTransform;
    [HideInInspector]
    public Vector3 toTargetV3;
    // public static Ship DefaultShip;
    public static Ship ActiveShip;
    readonly List<Transform> _jetsTransforms = new();
    readonly List<VisualEffect> _jetsVisualEffects = new();
    readonly List<VelocityEstimator> _jetsVelocityEstimators = new();
    [HideInInspector]
    public bool isFiring;
    // Collider _closestShipCollider;
    [HideInInspector]
    public Collider shipCollider;
    List<Weapon> _weapons = new();
    public static float ShootingRange = 50;
    public static float ShootingSqrRange;
    [HideInInspector]
    public float afterburnerCoefficient = 1;
    float _fastestWeaponSpeedMetersPerSecond;
    [HideInInspector]
    public Transform predictPositionDummyTransform;
    [HideInInspector]
    public VelocityEstimator velocityEstimator;
    [HideInInspector]
    public TurnType turnType;
    [HideInInspector]
    public LayerMask shootableLayerMasks;  // All layers that this ship shoots
    [HideInInspector]
    public Caption caption;
    float _forwardToTargetAngle;
    // float _collidedWithOtherShipAt;

    public enum TurnType    // Type of Ship rotation
    {
        Velocity,           // For other ships
        CustomTarget        // For player & ships with a target
    }

    void Start()
    {
        // ShipList.Add(this);
        ShootingSqrRange = Mathf.Pow(ShootingRange, 2);
        shipCollider = GetComponent<Collider>();
        velocityEstimator = GetComponent<VelocityEstimator>();
        _rotationSpeedVector = rotationSpeed * Vector3.up;
        // rb.automaticCenterOfMass = false;
        // rb.automaticInertiaTensor = false;
        // rb.centerOfMass = Vector3.zero;
        // rb.inertiaTensorRotation = Quaternion.identity;

        if (IsEnemy())
            EnemyShips.Add(this);

        if (!shipCollider.enabled)
            Debug.LogWarning("-- Disabled collider! --");

        PrepareWeapons();

        if (!IsPlayer() && !IsAstronaut())
            predictPositionDummyTransform = Instantiate(Uc.predictPositionDummyPrefab, UI.transform).transform;

        InitCaption();

        UpdateShootableLayerMasks();

        PrepareJets();
    }

    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        _rbTransform = rb.transform;
    }

    void Update()
    {
        UpdateToTargetV3();
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
        UpdateJets();
        ProcessConstraints();

        // UpdatePredictiveCollider();

        if (predictPositionDummyTransform)
            predictPositionDummyTransform.position = Uc.mainCamera.WorldToScreenPoint(transform.position + GetPredictedPositionOffset(this, rb.velocity, ActiveShip.gameObject, ActiveShip._fastestWeaponSpeedMetersPerSecond));
    }

    void ProcessConstraints()
    {
        // Rotation X and position Y constraints don't work together. This pushes object to position Y.
        rb.AddForce(200 * _rbTransform.position.y * Time.fixedDeltaTime * Vector3.down, ForceMode.Acceleration);
    }

    void PrepareWeapons()
    {
        var weapons = transform.Find("WEAPON_SLOTS");
        if (!weapons)
            return;

        foreach (Transform weapon in weapons)
        {
            var weaponComponent = weapon.GetComponent<Weapon>();

            if (!weaponComponent.enabled)
                continue;

            _weapons.Add(weaponComponent);

            var projectilePrefab = weaponComponent.projectilePrefab;
            var laserComponent = projectilePrefab.GetComponent<Laser>();
            float highestSpeed = laserComponent ? laserComponent.speed : 0;
            // speed = laserComponent ? laserComponent.speed : weaponComponent.GetComponent<Rocket>().speed;

            if (highestSpeed > _fastestWeaponSpeedMetersPerSecond)
                _fastestWeaponSpeedMetersPerSecond = highestSpeed;
        }
    }

    public void SetAsActiveShip()
    {
        if (ActiveShip)
        {
            ActiveShip.gameObject.layer = Uc.shootableShipsNeutralLayer;
            // ActiveShip.predictPositionDummyTransform  // TODO: Ten predictPositionDummyTransform by nakonec asi mohli mít všichni. Tady to zapínat/vypínat.
        }

        ActiveShip = this;

        gameObject.layer = Uc.shootablePlayerLayer;

        UpdateShootableLayerMasks();
    }

    void UpdateShootableLayerMasks()
    {
        if (IsPlayer())
            shootableLayerMasks = 1 << Uc.shootableEnvironmentLayer | 1 << Uc.shootableShipsEnemyLayer;
        else if (IsEnemy())
            shootableLayerMasks = 1 << Uc.shootableEnvironmentLayer | 1 << Uc.shootablePlayerLayer;
        else if (gameObject.layer == Uc.shootableShipsNeutralLayer)
            shootableLayerMasks = 1 << Uc.shootableEnvironmentLayer;
    }

    public bool IsPlayer()
    {
        return this == ActiveShip;
    }

    bool IsEnemy()
    {
        return gameObject.layer == Uc.shootableShipsEnemyLayer;
    }

    public void SetMoveVectorHorizontal(float horizontal)
    {
        moveVector.x = horizontal;
    }

    public void SetMoveVectorVertical(float vertical)
    {
        moveVector.z = vertical;
    }

    public float GetForwardSpeed()  // m / s
    {
        var velocity = rb.velocity;
        var forward = transform.forward;
        return Vector2.Dot(new(velocity.x, velocity.z), new(forward.x, forward.z));
    }

    void Move()
    {
        if (moveVector is { x: 0, z: 0 })
            return;

        var forwardness = Vector3.Dot(transform.forward, rb.velocity.normalized) / 4 + .75f;  // .5f .. 1

        rb.AddForce(SetVectorLength(moveVector, speed * afterburnerCoefficient * forwardness), ForceMode.Acceleration);
    }

    void Rotate()
    {
        // print("rotating ship");
        // yaw
        var forward = transform.forward;
        var toTargetNormalized = toTargetV3.normalized;
        _forwardToTargetAngle = - Vector2.SignedAngle(new(forward.x, forward.z), new(toTargetNormalized.x, toTargetNormalized.z));
        rb.AddTorque(_forwardToTargetAngle * _rotationSpeedVector, ForceMode.Acceleration);

        // var b = a >= 3 ? 10 : a <= -3 ? -10 : 0;
        // rb.AddTorque(b * rotationSpeed * Vector3.up, ForceMode.Acceleration);

        // if (this == ActiveShip)
        //     InfoText.text = "angle: " + _forwardToTargetAngle;

        // roll
        _rbTransform.localEulerAngles = new(0, transform.localEulerAngles.y, Mathf.Clamp(- 20 * rb.angularVelocity.y, - maxRollAngle, maxRollAngle));
    }

    void UpdateToTargetV3()
    {
        toTargetV3 = IsPlayer() || turnType == TurnType.CustomTarget ? _customTarget - transform.position : rb.velocity;
        Debug.DrawRay(transform.position, toTargetV3.normalized * 100, Color.cyan);
    }

    public void SetCustomTarget(Vector3 target)
    {
        _customTarget = target;
    }

    void PrepareJets()
    {
        var jets = transform.Find("JETS");
        if (!jets)
            return;

        foreach (Transform jetTransform in jets.transform)
        {
            _jetsTransforms.Add(jetTransform);
            _jetsVisualEffects.Add(jetTransform.GetComponent<VisualEffect>());
            _jetsVelocityEstimators.Add(jetTransform.AddComponent<VelocityEstimator>());
        }
        _jetAngleCos = - Mathf.Cos(jetAngle * Mathf.Deg2Rad);
    }

    void UpdateJets()
    {
        // if (!IsPlayer())
        //     InfoText.text = moveVector + "\n" + Mathf.Abs(_forwardToTargetAngle);  

        // if (moveVector is { x: 0, z: 0 } /*&& Mathf.Abs(_forwardToTargetAngle) < 1*/)
        // {
        //     DisableJets();
        //     return;
        // }

        var count = _jetsTransforms.Count;
        for (int i = 0; i < count; ++i)
        {
            var movement = _jetsVelocityEstimators[i].GetVelocityEstimate();

            if (movement.sqrMagnitude < .05f)
            {
                _jetsVisualEffects[i].SetBool("jet enabled", false);
                continue;
            }

            var jetForward = _jetsTransforms[i].forward;

            Vector2 v1 = new(movement.x, movement.z);
            Vector2 v2 = new(jetForward.x, jetForward.z);
            v1.Normalize();
            v2.Normalize();

            _jetsVisualEffects[i].SetBool("jet enabled", Vector2.Dot(v1, v2) < _jetAngleCos);
        }
    }

    void DisableJets()
    {
        foreach (VisualEffect vfx in _jetsVisualEffects)
            vfx.SetBool("jet enabled", false);
    }

    public ClosestShip GetClosestShipInRange(List<Ship> shipList, float range = Mathf.Infinity)
    {
        var closestSqrRange = Mathf.Pow(range, 2);
        ClosestShip closestShip = new(); 

        foreach (var ship in shipList)
        {
            if (this == ship)
                continue;

            var toClosestPointV3 = ActiveShip.GetVectorToClosestPoint(ship);
            var toClosestPointV3SqrMagnitude = toClosestPointV3.sqrMagnitude;
            if (toClosestPointV3SqrMagnitude < closestSqrRange)
            {
                closestShip.ship = ship;
                closestShip.toClosestPointV3 = toClosestPointV3;
                closestSqrRange = toClosestPointV3SqrMagnitude;
                // InfoText.text = toClosestPointV3SqrMagnitude.ToString();
            }
        }

        // if (closestShip.ship)
        //     InfoText.text = closestShip.ship.toTargetV3.magnitude.ToString();

        return closestShip;
    }

    public float GetSqrDistanceFromShipPosition(Ship otherShip, float range = Mathf.Infinity)  // range přesunout výše
    {
        var thisShipPosition = transform.position;
        otherShip.shipCollider.enabled = true;

        Physics.Raycast(thisShipPosition, otherShip.transform.position - thisShipPosition, out var hit, range, Uc.closestShipColliderLayer);

        otherShip.shipCollider.enabled = false;
        return (hit.point - thisShipPosition).sqrMagnitude;
    }

    Vector3 GetVectorToClosestPoint(Ship otherShip)
    {
        var thisShipPosition = transform.position;

        // var dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // dummy.transform.position = otherShip._collider.ClosestPoint(thisShipPosition);
        // dummy.GetComponent<Collider>().enabled = false;
        // InfoText.text = "distance: " + (otherShip._collider.ClosestPoint(thisShipPosition) - thisShipPosition).sqrMagnitude;
        return otherShip.shipCollider.ClosestPoint(thisShipPosition) - thisShipPosition;
    }

    public float GetSqrClosestDistanceToShip(Ship otherShip)
    {
        return GetVectorToClosestPoint(otherShip).sqrMagnitude;
    }

    bool IsAstronaut()
    {
        return this == Astronaut;
    }

    public static bool IsAstronautActive()
    {
        return Astronaut.gameObject.activeSelf;
    }

    public bool IsDefaultShipActive()
    {
        return ActiveShip.CompareTag("Default Ship");
    }

    void OnDestroy()
    {
        EnemyShips.Remove(this);
        Uc.canBeBoardedList.Remove(this);
    }

    float GetPredictedPositionZOffsetTime(Ship ship)
    {
        var target = ship;
        var bulletVelocity = ship.rb.velocity.magnitude;

        var targetTransform = target.transform;
        var targetVelocity = target.rb.velocity;

        Vector3 toTarget =  targetTransform.position - transform.position;
        float a = Vector3.Dot(targetVelocity, targetVelocity) - bulletVelocity * bulletVelocity;
        float b = 2 * Vector3.Dot(targetVelocity, toTarget);
        float c = Vector3.Dot(toTarget, toTarget);

        float p = - b / (2 * a);
        float q = Mathf.Sqrt(Mathf.Abs(b * b - 4 * a * c)) / (2 * a);

        float t1 = p - q;
        float t2 = p + q;
        float t;

        if (t1 > t2 && t2 > 0)
            t = t2;
        else
            t = t1;

        return Mathf.Abs(t);
    }

    /*void CreatePredictiveCollider()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // cube.transform.parent = transform;
        cube.transform.position = Vector3.zero;
        cube.GetComponent<Collider>().isTrigger = true;
        cube.layer = LayerMask.NameToLayer("predictive collider");
        cube.transform.localScale = 2 * _collider.bounds.extents;  // TODO: Jestli to tu zůstane, chtělo by to lossyScale
        _predictiveColliderTransform = cube.transform;
    }

    void UpdatePredictiveCollider()
    {
        var pos = GetPredictedPosition(this, ActiveShip.rb.velocity.magnitude);  // TODO: Pro každý pár je ta predikce jiná
        
        if (Double.IsNaN(pos.x) || Double.IsNaN(pos.y) || Double.IsNaN(pos.z))
            return;

        _predictiveColliderTransform.position = pos;
    }*/

    void InitCaption()
    {
        caption = Instantiate(Uc.captionPrefab, UI.transform);
        caption.GetComponent<Caption>().Setup(this);
    }

    public void UpdateCameraPosition()
    {
        var coef = Input.GetKey(KeyCode.LeftControl) ? .8f : .16f;

        MainCameraTransform.position = ActiveShip.transform.position +
                                       InitialCameraOffset + // vertical offset
                                       ActiveShip.toTargetV3 * coef; // horizontal offset
    }

    public void SetIsFiring(bool enable)
    {
        isFiring = enable;
    }

    // public void Highlight()
    // {
    //     universeController.selectionSprite.transform.position = transformCached.position;
    // }

    /*void OnCollisionEnter(Collision other)
    {
        if (IsPlayer() && moveVector is not { x: 0, z: 0 })
            return;

        _collidedWithOtherShipAt = Time.time;

        DisableJets();
    }

    void CheckCollidedWithOtherShipAt()
    {
        if (_collidedWithOtherShipAt == 0)
            return;
    }*/
}
