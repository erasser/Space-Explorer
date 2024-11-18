using UnityEngine;

public class FPSProjectile : MonoBehaviour
{
    [Tooltip("m/s")]
    public float speed = 40;
    [Tooltip("s")]
    public float delay = .2f;
    [HideInInspector]
    public Vector3 velocity;
    float _raycastLength;

    void Start()
    {
        _raycastLength = speed * Time.fixedDeltaTime;
        velocity = _raycastLength * Vector3.forward;
    }

    void Update()
    {
        transform.Translate(velocity);

        CheckCollision();
    }

    void CheckCollision()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hit, _raycastLength))
        {
            hit.collider.gameObject.GetComponent<FPSDamageable>()?.TakeDamage(10);

            Destroy(gameObject);
        }
    }
    

}
