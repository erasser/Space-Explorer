using UnityEngine;
// https://discussions.unity.com/t/trying-to-understand-rigidbody-forcemode-derivation/118383

// VelocityChange: ignoring mass & ZA SEKUNDU
// Acceleration:   ignoring mass & ZA SNÍMEK

public class PhysicsTest : MonoBehaviour
{
    public ForceMode forceMode;
    public bool addForce;
    public float forceValue = 1000;
    public bool addTorque;
    public float torqueValue = 100;
    Rigidbody _rb;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (addForce && forceMode == ForceMode.VelocityChange)
            _rb.AddForce(Vector3.right * forceValue * Time.fixedDeltaTime, forceMode);
        else if (addForce && forceMode == ForceMode.Acceleration)
            _rb.AddForce(Vector3.right * forceValue, forceMode);

        if (addTorque)
            _rb.AddTorque(Vector3.up * torqueValue, forceMode);
        
        
    }
}
