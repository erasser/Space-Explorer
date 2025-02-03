using System.Collections;
using UnityEngine;

public class Comet : MonoBehaviour
{
    public TrailRenderer trail;

    void Start()
    {
        StartCoroutine(DestroyMe());
    }

    void Update()
    {
        transform.Translate(10 * Time.deltaTime * Vector3.forward);
    }

    void OnDestroy()
    {
        trail.transform.SetParent(null);
    }

    IEnumerator DestroyMe()
    {
        yield return new WaitForSeconds(200);
        Destroy(gameObject);
    }
}
