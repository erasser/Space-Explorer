using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class Decal : MonoBehaviour
{
    public static List<DecalProjector> Decals = new();
    const int MaxCount = 200;
    const float Lifespan = 60;
    float _destroyAt;

    void Start()
    {
        _destroyAt = Time.time + Lifespan;
    }

    void Update()
    {
        if (Time.time > _destroyAt)
            Destroy(gameObject);

        if (Decals.Count > MaxCount)
        {
            Destroy(Decals[0].gameObject);
            Decals.RemoveAt(0);
        }
    }
}
