using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using static UniverseController;

public class Ship : CachedMonoBehaviour
{
    public static Ship ship;
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
    [HideInInspector]
    public Vector3 toTargetV3;
    public static Ship DefaultShip;
    public static Ship ActiveShip;
    // [SerializeField]
    // VisualEffect _fireVfx;  // To je vlastně weapon slot
    readonly List<Transform> _jetsTransforms = new();
    readonly List<VisualEffect> _jetsVisualEffects = new();
    public bool isFiring;
    public float forwardSpeed;

    void Start()
    {
        ship = this;
        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezePositionY;

        if (CompareTag("Default Ship"))
            DefaultShip = ActiveShip = this;

        foreach (Transform jetTransform in transform.Find("JETS").transform)
        {
            _jetsTransforms.Add(jetTransform);
            _jetsVisualEffects.Add(jetTransform.GetComponent<VisualEffect>());
        }

        jetAngle = - Mathf.Cos(jetAngle * Mathf.Deg2Rad);
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
        forwardSpeed = Vector2.Dot(new(_rb.velocity.x, _rb.velocity.z), new(transformCached.forward.x, transformCached.forward.z));

        Move();
        Rotate();
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

}
