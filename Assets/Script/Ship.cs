using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UniverseController;

// Ship requirements: This script,  Rigidbody, Collider, "ship" layer

public class Ship : CachedMonoBehaviour
{
    // public static Ship ship;  // 'ship' jsem už použil, přejmenovat
    public static readonly List<Ship> ShipList = new();
    [HideInInspector]
    public Vector3 moveVector;
    public float speed = 200;
    public float rotationSpeed = 3;
    [Range(0, 90)]
    public float maxRollAngle = 80;
    [Tooltip("Jet to ship angle in degrees.\nIt affects individual jet activation condition.\nHigher value => more jets\n\nDO NOT change in playmode!")]
    [Range(0, 90)]      // -.5 odpovídá 60 °, čili setupu tří jetů do hvězdy
    public float jetAngle = 45;
    float _jetAngleCos;
    Vector3 _userTarget;
    Rigidbody _rb;
    [HideInInspector]
    public Vector3 toTargetV3;
    // public static Ship DefaultShip;
    public static Ship ActiveShip;
    readonly List<Transform> _jetsTransforms = new();
    readonly List<VisualEffect> _jetsVisualEffects = new();
    public bool isFiring;
    Collider _closestShipCollider;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezePositionY;
        var cscGameObject = transformCached.Find("closest ship collider");
        if (cscGameObject)
        {
            _closestShipCollider = cscGameObject.GetComponent<Collider>();
            _closestShipCollider.enabled = false;
        }

        if (CompareTag("Default Ship"))
            /*DefaultShip =*/ ActiveShip = this;

        foreach (Transform jetTransform in transform.Find("JETS").transform)
        {
            _jetsTransforms.Add(jetTransform);
            _jetsVisualEffects.Add(jetTransform.GetComponent<VisualEffect>());
        }

        _jetAngleCos = - Mathf.Cos(jetAngle * Mathf.Deg2Rad);

        ShipList.Add(this);
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
        return Vector2.Dot(new(_rb.velocity.x, _rb.velocity.z), new(transformCached.forward.x, transformCached.forward.z));
    }

    void Update()
    {
        toTargetV3 = _userTarget - transformCached.position;
    }

    void Move()
    {
        if (moveVector is { x: 0, z: 0 })
        {
            // visualEffect.SetBool("jet enabled", false);
            DisableJets();
            return;
        }

        UpdateJets();

        _rb.AddForce(SetVectorLength(moveVector, speed * (Input.GetKey(KeyCode.LeftShift) ? 2 : 1)), ForceMode.Acceleration);
    }

    void Rotate()
    {
        // yaw
        _rb.AddTorque(- Vector2.SignedAngle(new(transformCached.forward.x, transformCached.forward.z), new(toTargetV3.x, toTargetV3.z)) * rotationSpeed * Vector3.up, ForceMode.Acceleration);

        // roll
        _rb.transform.localEulerAngles = new(0, _rb.transform.localEulerAngles.y, Mathf.Clamp(- 20 * _rb.angularVelocity.y, - maxRollAngle, maxRollAngle));
    }

    public void SetUserTarget(Vector3 target)
    {
        _userTarget = target;
    }

    void UpdateJets()
    {
        var movement = moveVector.normalized;
        var jetCount = _jetsTransforms.Count;

        for (int i = 0; i < jetCount; ++i)
        {
            // _jetsVisualEffects[i].SetBool("jet enabled", true);
            // continue;
            var jetForward = _jetsTransforms[i].forward;
            float dot = Vector3.Dot(new(movement.x, movement.z), new(jetForward.x, jetForward.z));

            if (dot < _jetAngleCos)
                _jetsVisualEffects[i].SetBool("jet enabled", true);
            else
                _jetsVisualEffects[i].SetBool("jet enabled", false);
        }
    }

    void DisableJets()
    {
        foreach (VisualEffect vfx in _jetsVisualEffects)
            vfx.SetBool("jet enabled", false);
    }

    public Ship GetClosestShipInRange(float range = Mathf.Infinity)
    {
        Ship closestShip = null;
        var closestSqrRange = range * range;

        foreach (var ship in ShipList)
        {
            if (ActiveShip == ship)
                continue;

            var sqrDistance = ActiveShip.GetSqrDistanceFromShip(ship);

            if (sqrDistance < closestSqrRange)
            {
                closestShip = ship;
                closestSqrRange = sqrDistance;
            }
        }

        return closestShip;
    }

    public float GetSqrDistanceFromShip(Ship otherShip, float range = Mathf.Infinity)
    {
        var thisShipPosition = transformCached.position;
        otherShip._closestShipCollider.enabled = true;

        Physics.Raycast(thisShipPosition, otherShip.transformCached.position - thisShipPosition, out var hit, range, universeController.closestShipColliderLayer);

        otherShip._closestShipCollider.enabled = false;
        return (hit.point - thisShipPosition).sqrMagnitude;
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

    // void Highlight()
    // {
    //     universeController.selectionSprite.transform.position = transformCached.position;
    // }


}
