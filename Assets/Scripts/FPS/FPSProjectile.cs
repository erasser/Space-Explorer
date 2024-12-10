using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using static Decal;
using Random = UnityEngine.Random;
using static WorldController;

public class FPSProjectile : MonoBehaviour
{
    [Tooltip("m/s")]
    public float speed = 100;
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
    public int bouncesCount;
    int _bouncesDone;
    public float damage = 10;
    Vector3 _lastPosition;  // Raycastuje se z poslední pozice po uplynutí deltaTime

    void Start()
    {
        velocity = speed * Vector3.forward;
        kineticEnergy *= - 1;
        _decalDepth = decalPrefab.GetComponent<DecalProjector>().size.z;
        _destroyAt = Time.time + lifespan;
    }

    void Update()
    {
        if (Time.time > _destroyAt)
            Destroy(gameObject);

        CheckCollision();

        transform.Translate(velocity * Time.deltaTime);
    }

    void CheckCollision()
    {
        if (Physics.Raycast(_lastPosition, transform.forward, out var hit, speed * Time.deltaTime))  // Layers must correspond with Raycast in FPSWeapon.Shoot()
        {
            ++_bouncesDone;

            // hit.collider.gameObject.GetComponent<FPSDamageable>()?.TakeDamage(damage / (_bouncesDone + 1));  // TODO: Spíš udělám nadtřídu pro FPSPlayer a FPSEnemy

            var rb = hit.collider.gameObject.GetComponent<Rigidbody>();
            if (rb)
                rb.AddForceAtPosition(kineticEnergy / (_bouncesDone + 1) * hit.normal, hit.point);

            var reflectionVector = Vector3.Reflect(transform.forward, hit.normal);

            if (hit.collider.gameObject.name != "PLAYER")
                CreateDecal(hit, reflectionVector);

            Wc.LaunchHitEffect(hit.point, reflectionVector);

            if (_bouncesDone > bouncesCount)
                Destroy(gameObject);
            else
                transform.LookAt(transform.position + reflectionVector);
        }

        _lastPosition = transform.position;
    }

    void CreateDecal(RaycastHit hit, Vector3 dir)
    {
        var decal = Instantiate(decalPrefab, hit.point, Quaternion.LookRotation(- dir));
        var decalProjector = decal.GetComponent<DecalProjector>();

        decal.transform.eulerAngles = new (decal.transform.eulerAngles.x, decal.transform.eulerAngles.y, Random.Range(0, 360));
        decal.transform.SetParent(hit.collider.gameObject.transform);

        var size = Random.Range(.06f, .2f);
        decalProjector.size = new (size, size, _decalDepth);

        Decals.Add(decalProjector);
    }


}
