using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UniverseController;

public class Ship : CachedMonoBehaviour
{
    // public static Ship ship;  // 'ship' jsem už použil, přejmenovat
    public static readonly List<Ship> ShipList = new();
    public Vector3 moveVector;
    public float speed = 40000;
    public float rotationSpeed = 300;
    [Range(0, 90)]
    public float maxRollAngle = 80;
    [Tooltip("Jet to ship angle in degrees.\nIt affects individual jet activation condition.\n\nLower value => less jet")]
    [Range(0, 90)]      // -.5 odpovídá 60 °, čili setupu tří jetů do hvězdy
    public float jetAngle = 45;
    Vector3 _userTarget;
    Rigidbody _rb;
    Collider _collider;
    [HideInInspector]
    public Vector3 toTargetV3;
    public static Ship DefaultShip;
    public static Ship ActiveShip;
    readonly List<Transform> _jetsTransforms = new();
    readonly List<VisualEffect> _jetsVisualEffects = new();
    public bool isFiring;

    void Start()
    {
        // ship = this;
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezePositionY;
        _collider = GetComponent<Collider>();  // TODO: To bude problém, když objekt bude mít více colliderů

        if (CompareTag("Default Ship"))
            DefaultShip = ActiveShip = this;

        foreach (Transform jetTransform in transform.Find("JETS").transform)
        {
            _jetsTransforms.Add(jetTransform);
            _jetsVisualEffects.Add(jetTransform.GetComponent<VisualEffect>());
        }

        jetAngle = - Mathf.Cos(jetAngle * Mathf.Deg2Rad);

        foreach (Ship ship in FindObjectsByType<Ship>(FindObjectsSortMode.None))
            ShipList.Add(ship);
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
        // visualEffect.SetFloat("Player speed", ActiveShip.rigidBody.velocity.magnitude);
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

        _rb.AddForce(SetVectorLength(moveVector, speed));
    }

    void Rotate()
    {
        toTargetV3 = _userTarget - transformCached.position;

        // yaw
        _rb.AddTorque(- Vector2.SignedAngle(new(transformCached.forward.x, transformCached.forward.z), new(toTargetV3.x, toTargetV3.z)) * rotationSpeed * Vector3.up);

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
            var jetForward = _jetsTransforms[i].forward;
            float dot = Vector3.Dot(new(movement.x, movement.z), new(jetForward.x, jetForward.z));

            if (dot < jetAngle)
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

    public Ship GetClosestShipInRange(float range)
    {
        if (!IsAstronautActive())
            return null;

        Ship closestShip = null;
        var closestSqrRange = range * range;

        foreach (var ship in ShipList)
        {
            if (ActiveShip == ship)
                continue;

            var r = ActiveShip.GetSqrDistanceFromShip(ship);

            if (r < closestSqrRange)
            {
                closestShip = ship;
                closestSqrRange = r;
            }
        }
        // print("closest ship: " + closestShip);
        return closestShip;
    }

    public float GetSqrDistanceFromShip(Ship ship)
    {
        RaycastHit hit;
        var pos = transformCached.position;

        Physics.Raycast(new(pos, ship.transformCached.position - pos), out hit);

        InfoText.text = ship.name + ": " + Mathf.Round((hit.point - pos).sqrMagnitude);

        // if (ship)
        //     print("• ship name: " + ship.name + " • sqrDistance: " + (hit.point - pos).sqrMagnitude);

        // GameObject.Find("Sphere").transform.position = hit.point;
        
        return (hit.point - pos).sqrMagnitude;
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












}
