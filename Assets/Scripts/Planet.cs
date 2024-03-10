using UnityEngine;

public class Planet : MonoBehaviour
{
    [Tooltip("Degrees / s")]
    public float rotationSpeed = 1;
    public float atmosphereRelativeRotationSpeed = .4f;
    [Tooltip("Should be child of the planet")]
    public Transform atmosphere;

    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        atmosphere.Rotate(Vector3.up, atmosphereRelativeRotationSpeed * Time.deltaTime);
    }
}
