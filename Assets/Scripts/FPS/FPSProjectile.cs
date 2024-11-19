using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static Decal;
using Random = UnityEngine.Random;
using static WorldController;

public class FPSProjectile : MonoBehaviour
{
    [Tooltip("m/s")]
    public float speed = 40;
    [Tooltip("s")]
    public float delay = .2f;
    [HideInInspector]
    public Vector3 velocity;
    float _raycastLength;
    public GameObject decalPrefab;
    public float kineticEnergy = 1000;
    float _decalDepth;
    public float lifespan = 10;
    float _destroyAt;

    void Start()
    {
        _raycastLength = speed * Time.fixedDeltaTime;
        velocity = _raycastLength * Vector3.forward;
        kineticEnergy *= -1;
        _decalDepth = decalPrefab.GetComponent<DecalProjector>().size.z;
        _destroyAt = Time.time + lifespan;
    }

    void Update()
    {
        if (Time.time > _destroyAt)
            Destroy(gameObject);

        transform.Translate(velocity);

        CheckCollision();
    }

    void CheckCollision()
    {
        if (Physics.Raycast(transform.position, transform.forward, out var hit, _raycastLength))  // Layers must correspond with Raycast in FPSWeapon.Shoot()
        {
            hit.collider.gameObject.GetComponent<FPSDamageable>()?.TakeDamage(10);
            
            var rb = hit.collider.gameObject.GetComponent<Rigidbody>();
            if (rb)
                rb.AddForceAtPosition(kineticEnergy * hit.normal, hit.point);

            CreateDecal(hit);

            Wc.LaunchHitEffect(hit.point, Vector3.Reflect(transform.forward, hit.normal));

            Destroy(gameObject);
        }
    }

    void CreateDecal(RaycastHit hit)
    {
        var decal = Instantiate(decalPrefab, hit.point, Quaternion.LookRotation(- hit.normal));
        var decalProjector = decal.GetComponent<DecalProjector>();

        decal.transform.eulerAngles = new (decal.transform.eulerAngles.x, decal.transform.eulerAngles.y, Random.Range(0, 360));
        decal.transform.SetParent(hit.collider.gameObject.transform);

        var size = Random.Range(.06f, .2f);
        decalProjector.size = new (size, size, _decalDepth);

        Decals.Add(decalProjector);
    }


}
