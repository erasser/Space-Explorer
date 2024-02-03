using System;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Laser : Projectile
{
    // Vector3 _velocity;  // m / s

    /*public void Setup(Vector3 position, float rotationY, Vector3 speed)
    {
        _speedV3 = speed;
        _sqrRaycastLength = speed.z;  // TODO: Myslet na slow-motion, mělo by obsahovat Time.fixedDeltaTime a po přechodu do slow-mo updatovat - to se asi týká jen už vystřelených projektilů  
        _selfDestructAtTime = Time.time + weapon.range / (_speedV3.z / Time.fixedDeltaTime);
        transform.position = position;

        if (weapon.autoAim)
            transform.LookAt(MouseCursorHit.point);
        else
            transform.rotation = Quaternion.Euler(new(0, rotationY, 0));  // TODO: Nedalo by se to nějak zjednodušit? :D
    }*/

    // void Start()
    // {
    //     _velocity = speed * Time.fixedDeltaTime * Vector3.forward;
    // }

    // void FixedUpdate()
    // {
    //     UpdateTransform();
    // }

    // void UpdateTransform()
    // {
    //     transform.Translate(velocity);
    // }

}
