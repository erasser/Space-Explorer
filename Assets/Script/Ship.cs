using System;
using UnityEngine;
using UnityEngine.VFX;
using static UniverseController;

public class Ship : CachedMonoBehaviour
{
    public static Ship ship;
    public Rigidbody _rb; /***/
    Vector3 _moveVector;
    public const float Speed = 20000;
    public const float RotationSpeed = 150;
    Vector3 _userTarget;
    [HideInInspector]
    public Vector3 toTargetV3;
    public static Ship DefaultShip;
    public static Ship ActiveShip;
    // [SerializeField]
    // VisualEffect visualEffect;  // To bude stačit jen zapnout / vypnout

    void Start()
    {
        ship = this;
        _rb = GetComponent<Rigidbody>();
        
        if (CompareTag("Default Ship"))
            DefaultShip = ActiveShip = this;
    }

    public void SetMoveVectorHorizontal(float horizontal)
    {
        _moveVector.x = horizontal;
    }

    public void SetMoveVectorVertical(float vertical)
    {
        _moveVector.z = vertical;
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
    }

    void Update()
    {
        // visualEffect.SetFloat("Player speed", ActiveShip.rigidBody.velocity.magnitude);
    }

    void Move()
    {
        if (_moveVector is { x: 0, z: 0 })
            return;

        _rb.AddForce(SetVectorLength(_moveVector, Speed));
    }

    void Rotate()
    {
        toTargetV3 = _userTarget - transformCached.position;

        _rb.AddTorque(- Vector2.SignedAngle(new(transformCached.forward.x, transformCached.forward.z), new(toTargetV3.x, toTargetV3.z)) * RotationSpeed * Vector3.up);
    }

    public void SetUserTarget(Vector3 target)
    {
        _userTarget = target;
    }

}
