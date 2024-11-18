using UnityEngine;

public class FPSWeapon : MonoBehaviour
{
    public FPSProjectile projectilePrefab;
    Transform _barrel;

    void Start()
    {
        _barrel = transform.Find("barrel");
    }

    void Update()
    {
        
    }

    public void Shoot()
    {
        Instantiate(projectilePrefab, _barrel.position, _barrel.rotation);
    }
}
