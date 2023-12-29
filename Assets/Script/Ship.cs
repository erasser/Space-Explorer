using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UniverseController;

// Ship requirements: This script, "ship" layer (to můžu asi udělat dynamicky)

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Ship : CachedMonoBehaviour
{
    public float speed = 10;
    public float rotationSpeed = 3;
    [Range(0, 90)]
    public float maxRollAngle = 80;
    [Tooltip("Jet to ship angle in degrees.\nIt affects individual jet activation condition.\nHigher value => more jets\n\nDO NOT change in playmode!")]
    [Range(0, 90)]      // -.5 odpovídá 60 °, čili setupu tří jetů do hvězdy
    public float jetAngle = 45;
    static readonly List<Ship> ShipList = new();
    [HideInInspector]
    public Vector3 moveVector;
    float _jetAngleCos;
    Vector3 _userTarget;
    [HideInInspector]
    public Rigidbody rb;
    [HideInInspector]
    public Vector3 toTargetV3;
    // public static Ship DefaultShip;
    public static Ship ActiveShip;
    readonly List<Transform> _jetsTransforms = new();
    readonly List<VisualEffect> _jetsVisualEffects = new();
    [HideInInspector]
    public bool isFiring;
    // Collider _closestShipCollider;
    Collider _collider;
    int _jetCount;
    List<Weapon> _weapons = new();
    public static float ShootingRange = 40;
    public static float ShootingSqrRange;
    [HideInInspector]
    public float afterburnerCoefficient = 1;

    void Start()
    {
        ShipList.Add(this);
        ShootingSqrRange = Mathf.Pow(ShootingRange, 2);
        _collider = GetComponent<Collider>();
        if (!_collider.enabled)
            Debug.LogWarning("-- Disabled collider! --");

        // var cscGameObject = transformCached.Find("closest ship collider");
        // if (cscGameObject)
        // {
        //     _closestShipCollider = cscGameObject.GetComponent<Collider>();
        //     // _closestShipCollider.enabled = false;
        // }

        if (CompareTag("Default Ship"))
            /*DefaultShip =*/ ActiveShip = this;

        var jets = transform.Find("JETS");
        if (jets)
        {
            foreach (Transform jetTransform in jets.transform)
            {
                _jetsTransforms.Add(jetTransform);
                _jetsVisualEffects.Add(jetTransform.GetComponent<VisualEffect>());
            }
            _jetCount = _jetsTransforms.Count;
            _jetAngleCos = - Mathf.Cos(jetAngle * Mathf.Deg2Rad);
        }

        var weapons = transform.Find("WEAPON_SLOTS");
        if (weapons)
            foreach (Transform weapon in weapons)
                _weapons.Add(weapon.GetComponent<Weapon>());
    }

    void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void SetMoveVectorHorizontal(float horizontal)
    {
        moveVector.x = horizontal;
    }

    public void SetMoveVectorVertical(float vertical)
    {
        moveVector.z = vertical;
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
    }

    public float GetForwardSpeed()  // m / s
    {
        var velocity = rb.velocity;
        var forward = transformCached.forward;
        return Vector2.Dot(new(velocity.x, velocity.z), new(forward.x, forward.z));
    }

    void Update()
    {
        toTargetV3 = ActiveShip == this ? _userTarget - transformCached.position : rb.velocity;
    }

    void Move()
    {
        if (moveVector is { x: 0, z: 0 })
        {
            DisableJets();
            return;
        }

        UpdateJets();

        // if (name == "ship_StarSparrow11")
        // {
        //     InfoText.text = SetVectorLength(moveVector, speed) .ToString();
        //     InfoText.text += "\n" + rb.velocity.magnitude;
        // }

        rb.AddForce(SetVectorLength(moveVector, speed * afterburnerCoefficient), ForceMode.Acceleration);
    }

    void Rotate()
    {
        // yaw
        var forward = transformCached.forward;
        var toTargetNormalized = toTargetV3.normalized;
        rb.AddTorque(- Vector2.SignedAngle(new(forward.x, forward.z), new(toTargetNormalized.x, toTargetNormalized.z)) * rotationSpeed * Vector3.up, ForceMode.Acceleration);

        // var a = - Vector2.SignedAngle(new(forward.x, forward.z), new(toTargetNormalized.x, toTargetNormalized.z));
        // var b = a >= 3 ? 10 : a <= -3 ? -10 : 0;
        // rb.AddTorque(b * rotationSpeed * Vector3.up, ForceMode.Acceleration);
        // InfoText.text = "angle: " + a;

        // roll
        rb.transform.localEulerAngles = new(0, transformCached.localEulerAngles.y, Mathf.Clamp(- 20 * rb.angularVelocity.y, - maxRollAngle, maxRollAngle));
    }

    public void SetUserTarget(Vector3 target)
    {
        _userTarget = target;
    }

    void UpdateJets()
    {
        var movement = moveVector.normalized;

        for (int i = 0; i < _jetCount; ++i)
        {
            var jetForward = _jetsTransforms[i].forward;

            _jetsVisualEffects[i].SetBool("jet enabled", Vector3.Dot(new(movement.x, movement.z), new(jetForward.x, jetForward.z)) < _jetAngleCos);
        }
    }

    void DisableJets()
    {
        foreach (VisualEffect vfx in _jetsVisualEffects)
            vfx.SetBool("jet enabled", false);
    }

    public ClosestShip GetClosestShipInRange(float range = Mathf.Infinity)
    {
        var closestSqrRange = Mathf.Pow(range, 2);
        ClosestShip closestShip = new(); 

        foreach (var ship in universeController.canBeBoardedList)
        {
            if (ActiveShip == ship)
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
        var thisShipPosition = transformCached.position;
        otherShip._collider.enabled = true;

        Physics.Raycast(thisShipPosition, otherShip.transformCached.position - thisShipPosition, out var hit, range, universeController.closestShipColliderLayer);

        otherShip._collider.enabled = false;
        return (hit.point - thisShipPosition).sqrMagnitude;
    }

    Vector3 GetVectorToClosestPoint(Ship otherShip)
    {
        var thisShipPosition = transformCached.position;

        // var dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // dummy.transform.position = otherShip._collider.ClosestPoint(thisShipPosition);
        // dummy.GetComponent<Collider>().enabled = false;
        // InfoText.text = "distance: " + (otherShip._collider.ClosestPoint(thisShipPosition) - thisShipPosition).sqrMagnitude;
        return otherShip._collider.ClosestPoint(thisShipPosition) - thisShipPosition;
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
        ShipList.Remove(this);
    }

    // public void Highlight()
    // {
    //     universeController.selectionSprite.transform.position = transformCached.position;
    // }

}
