using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Decal : MonoBehaviour
{
    public static List<DecalProjector> Decals = new();
    const int MaxCount = 100;
    const float Lifespan = 30;
    float _destroyAt;

    void Start()
    {
        _destroyAt = Time.time + Lifespan;
    }

    void Update()
    {
        if (Time.time > _destroyAt)
        {
            Destroy(gameObject);
            Decals.Remove(GetComponent<DecalProjector>());
        }

        if (Decals.Count > MaxCount)
        {
            Destroy(Decals[0].gameObject);
            Decals.RemoveAt(0);
        }
    }
}
