using System.Collections;
using UnityEngine;
using static WorldController;

public class FPSWeapon : MonoBehaviour
{
    public FPSProjectile projectilePrefab;
    Transform _barrel;
    GameObject _fireEffect;
    public float orientationSpeed = 10;

    void Start()
    {
        _barrel = transform.Find("barrel");
        _fireEffect = transform.Find("fire effect").gameObject;
        _fireEffect.SetActive(false);
    }

    void Update()
    {
        UpdateWeaponOrientation();
    }

    public void Shoot()
    {
        Instantiate(projectilePrefab, _barrel.position, _barrel.rotation);
        _fireEffect.SetActive(true);
        _fireEffect.transform.localEulerAngles = new(0, 0, Random.Range(0, 360));
        StartCoroutine(WaitAndHideEffect());
    }

    IEnumerator WaitAndHideEffect()
    {
        yield return new WaitForSeconds(.08f);
        _fireEffect.SetActive(false);
    }

    void UpdateWeaponOrientation()
    {
        var pointInWorld = FPSCamera.ScreenToWorldPoint(ScreenHalfResolution);

        if (Physics.Raycast(pointInWorld, FPSCamera.transform.forward, out var hit))
        {
            var startVector = transform.forward;
            var endVector = hit.point - transform.position;
            var resultVector = Vector3.RotateTowards(startVector, endVector, Time.deltaTime * orientationSpeed, 0);
            transform.LookAt(transform.position + resultVector);
        }
        else
            transform.localRotation = Quaternion.identity;

    }
}
