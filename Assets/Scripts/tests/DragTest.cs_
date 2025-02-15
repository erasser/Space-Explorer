using UnityEngine;

public class DragTest : MonoBehaviour
{
    public Rigidbody rb;
    public float force;
    public float initialForce;
    public Vector3 disabledEnginesAtPosition;
    float _brakingDistance;
    public float d;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialForce = force;
        force = 0;
    }

    public virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            force = initialForce;
    }

    public virtual void FixedUpdate()
    {
        rb.AddForce(force * transform.forward);
    }
}
