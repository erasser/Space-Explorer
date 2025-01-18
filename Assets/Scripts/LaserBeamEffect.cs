using UnityEngine;

public class LaserBeamEffect : MonoBehaviour
{
    Vector2 _initialScale;
    
    void Start()
    {
        _initialScale = transform.localScale;
    }

    void Update()
    {
        // transform.rotation = Random.rotation;
        transform.localScale = _initialScale * Random.Range(.9f, 1.1f);
    }
}
