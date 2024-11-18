using System.Collections;
using UnityEngine;

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
    }

    void Update()
    {
        
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
}
