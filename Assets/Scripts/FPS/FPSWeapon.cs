using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using static WorldController;

public class FPSWeapon : MonoBehaviour  // TODO: Rozšířit na EnemyWeapon and PlayerWeapon
{
    public FPSProjectile projectilePrefab;
    public Transform barrel;
    GameObject _fireEffect;
    public float weaponOrientationSpeed = 10;
    public float recoilAngle = 5;

    void Start()
    {
        barrel = transform.Find("barrel");
        _fireEffect = transform.Find("fire effect").gameObject;
        _fireEffect.SetActive(false);
    }

    public void Shoot()
    {
        ApplyRecoil(Instantiate(projectilePrefab, barrel.position, barrel.rotation));
        _fireEffect.SetActive(true);
        _fireEffect.transform.localEulerAngles = new(0, 0, Random.Range(0, 360));
        StartCoroutine(WaitAndHideEffect());
    }

    IEnumerator WaitAndHideEffect()
    {
        yield return new WaitForSeconds(.08f);
        _fireEffect.SetActive(false);
    }

    void ApplyRecoil(FPSProjectile shot)
    {
        if (transform.root.name == "PLAYER")  // TODO
            return;

        var tr = shot.transform;
        tr.Rotate(tr.right, Random.Range(- recoilAngle, recoilAngle));
        tr.Rotate(tr.up, Random.Range(- recoilAngle, recoilAngle));
    }
}
