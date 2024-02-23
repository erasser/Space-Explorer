using UnityEngine;

public class Planet : CachedMonoBehaviour
{
    [Tooltip("Degrees / s")]
    public float rotationSpeed = 1;
    public float atmosphereRelativeRotationSpeed = .4f;
    [Tooltip("Should be child of the planet")]
    public Transform atmosphere;

    void Start()
    {

    }

    void Update()
    {
        transformCached.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        atmosphere.Rotate(Vector3.up, atmosphereRelativeRotationSpeed * Time.deltaTime);
    }
}
