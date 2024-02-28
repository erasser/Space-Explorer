using UnityEngine;
// TODO: Když ztratí target, najít nový


public class Rocket : Projectile
{
    // [Range(0, 200)]
    // public float maxSpeed = 20;
    [Range(0, 20)]
    public float rotationSpeed = 2;
    Transform _target;
    Transform _dummyTarget;  // if no target is available

    void Awake()
    {
        _dummyTarget = new GameObject().transform;
        _dummyTarget.name = "- DUMMY TARGET -";
    }
    
    // new void Start()
    // {
    //     base.Start();
    // }

    new void FixedUpdate()
    {
        if (!_target)  // target destroyed by something else
            SetDummyTarget();

        base.FixedUpdate();

        UpdateRotation();
    }

    public void SetTarget(Ship target)
    {
        if (target)
            _target = target.transform;
        else
            SetDummyTarget();
    }

    void SetDummyTarget()
    {
        _dummyTarget.position = transform.position + transform.forward * range;
        _target = _dummyTarget;
        // print("Setting dummy target to: " + _dummyTarget.position); // TODO: WTF?
    }

    void UpdateRotation()
    {
        // print("target: " + _target);
        var toTargetVector = _target.position - transform.position;
        // var cross = Vector3.Cross(toTargetVector, transform.forward);
        // _rb.AddTorque(- 2 * cross.magnitude * cross.normalized, ForceMode.Force);  // Force is proportional to the angle

        var angle = Vector3.SignedAngle(toTargetVector, transform.forward, Vector3.down);

        if (angle > - rotationSpeed && angle < rotationSpeed)
            return;

        transform.Rotate(Vector3.up, rotationSpeed * Mathf.Sign(angle));
    }

    void OnDestroy()
    {
        DestroyImmediate(_dummyTarget.gameObject);
    }
}
