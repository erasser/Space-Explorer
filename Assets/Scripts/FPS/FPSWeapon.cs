using System.Collections;
using UnityEngine;
using static WorldController;

public class FPSWeapon : MonoBehaviour
{
    public FPSProjectile projectilePrefab;
    Transform _barrel;
    GameObject _fireEffect;

    void Start()
    {
        _barrel = transform.Find("barrel");
        _fireEffect = transform.Find("fire effect").gameObject;
        _fireEffect.SetActive(false);

        // _tmpHelper = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        // Destroy(_tmpHelper.gameObject.GetComponent<SphereCollider>());
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
        yield return new WaitForSeconds(.1f);
        _fireEffect.SetActive(false);
    }

    void UpdateWeaponOrientation()  // TODO: RotateTowards / SLERP pro zjemnění
    {
        var pointInWorld = FPSCamera.ScreenToWorldPoint(ScreenHalfResolution);

        if (Physics.Raycast(pointInWorld, FPSCamera.transform.forward, out var hit))
            transform.LookAt(hit.point);
        else
            transform.localRotation = Quaternion.identity;

    }
}
