using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
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
    Vector3 _userTarget;
    Rigidbody _rb;
    [HideInInspector]
    public Vector3 toTargetV3;
    public static Ship DefaultShip;
    public static Ship ActiveShip;
    [SerializeField]
    VisualEffect visualEffect;

    void Start()
    {
        ship = this;
        _rb = GetComponent<Rigidbody>();
        visualEffect.SetBool("jet enabled", false);
        
        if (CompareTag("Default Ship"))
            DefaultShip = ActiveShip = this;
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

    void Update()
    {
        // visualEffect.SetFloat("Player speed", ActiveShip.rigidBody.velocity.magnitude);
    }

    void Move()
    {
        if (moveVector is { x: 0, z: 0 })
        {
            visualEffect.SetBool("jet enabled", false);
            return;
        }

        visualEffect.SetBool("jet enabled", true);

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

}
