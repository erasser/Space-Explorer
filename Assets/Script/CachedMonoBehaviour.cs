using UnityEngine;

public class CachedMonoBehaviour : MonoBehaviour
{
    [HideInInspector]
    public Transform transformCached;
    [HideInInspector]
    public GameObject gameObjectCached;
    [HideInInspector]
    public Rigidbody rigidBodyCached;

    void Awake()
    {
        gameObjectCached = gameObject;
        transformCached = gameObjectCached.transform;
    }
}
