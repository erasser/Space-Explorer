using UnityEngine;
using static UniverseController;

public class Ship : CachedMonoBehaviour
{
    public static Ship ship;
    Rigidbody _rb;
    Vector3 _moveVector;
    public const float Speed = 20000;
    public const float RotationSpeed = 150;
    Vector3 _userTarget;
    [HideInInspector]
    public Vector3 toTargetV3;
    public static Ship ActiveShip;

    void Start()
    {
        ship = this;
        _rb = GetComponent<Rigidbody>();
        
        if (CompareTag("Default Ship"))
            ActiveShip = this;
    }

    public void SetMoveVectorHorizontal(float horizontal)
    {
        _moveVector.x = horizontal;
    }

    public void SetMoveVectorVertical(float vertical)
    {
        print("W 2");
        _moveVector.z = vertical;
    }

    void FixedUpdate()
    {
        Move();
        Rotate();
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
