using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Laser : Projectile
{
    new void FixedUpdate()
    {
        base.FixedUpdate();

        UpdatePosition();
    }

    void UpdatePosition()
    {
        transform.Translate(velocity);
    }

}
