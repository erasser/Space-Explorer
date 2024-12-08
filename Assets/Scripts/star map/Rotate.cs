using UnityEngine;

public class Rotate : MonoBehaviour
{
    public float rotationSpeed = 1;

    void Update()
    {
        transform.Rotate(Vector3.down, Time.deltaTime * rotationSpeed, Space.World);
    }
}
