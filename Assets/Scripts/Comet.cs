using System.Collections;
using UnityEngine;
using static UniverseController;
using static MyMath;

public class Comet : MonoBehaviour
{
    public TrailRenderer trail;
    public static bool FirstComet = true;

    void Start()
    {
        Setup();

        StartCoroutine(DestroyMe());

        StartCoroutine(CheckDistance());

        if (FirstComet)
            FirstComet = false;
    }

    void Setup()
    {
        trail.time = Random.Range(10f, 16f);

        var distance = Random.Range(400, 800);

        Vector3 screenPoint = FirstComet ? new(Random.Range(0, Screen.width / 2f), Random.Range(0, Screen.height / 2f), distance) : new(0, 0, distance);
        var offset = FirstComet ? Vector3.zero : new Vector3(100, 0, 100); 

        var position = MainCamera.ScreenToWorldPoint(screenPoint);
        position -= offset;                  // translate off-screen

        var rotateAround = MainCamera.ScreenToWorldPoint(new(Screen.width / 2f, Screen.height / 2f, distance));
        var radius = (position - rotateAround).magnitude;
        var angle = Random.Range(0f, TwoPI);

        var x = rotateAround.x + radius * Mathf.Cos(angle);
        var z = rotateAround.z + radius * Mathf.Sin(angle);

        transform.position = new(x, position.y, z);

        var targetRange = radius / 2;
        var targetX = rotateAround.x + Random.Range(- targetRange, targetRange);
        var targetZ = rotateAround.z + Random.Range(- targetRange, targetRange);
        Vector3 targetPoint = new(targetX, position.y, targetZ);
        transform.LookAt(targetPoint);
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

    IEnumerator CheckDistance()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);

            if ((SetVectorYToZero(transform.position) - SetVectorYToZero(MainCameraTransform.position)).sqrMagnitude > 2e6)  // player is too far
            {
                Destroy(gameObject);
                Uc.CreateComet();
            }
        }
    }
}
