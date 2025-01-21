using UnityEngine;

public class WeaponBeam : MonoBehaviour
{
    Ship _ship;
    public float chargingTime;
    public float lifeTime;
    public float shootInterval;
    public GameObject beamGameObject;

    float _shotAt;

    void Start()
    {
        _ship = transform.root.gameObject.GetComponent<Ship>();
    }

    void Update()
    {
        CheckFire();
    }

    void CheckFire()
    {
        beamGameObject.SetActive(_ship.isFiring);
        return;
        
        if (!_ship.isFiring)
            return;
        
        _shotAt = Time.time;

    }
}
