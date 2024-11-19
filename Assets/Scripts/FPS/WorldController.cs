using UnityEngine;
using UnityEngine.VFX;
using static FPSPlayer;

public class WorldController : MonoBehaviour
{
    public static WorldController Wc;
    public static Camera FPSCamera;
    public static Vector2 ScreenHalfResolution;
    [SerializeField]
    public VisualEffect bulletExplosionEffectPrefab;
    [HideInInspector]
    public VisualEffect bulletExplosionEffect;

    void Start()
    {
        Wc = this;
        Application.targetFrameRate = 60;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        ScreenHalfResolution = new(Screen.width / 2, Screen.height / 2);
        bulletExplosionEffect = Instantiate(bulletExplosionEffectPrefab);
    }

    void Update()
    {
        ProcessControls();
    }

    void ProcessControls()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            SceneController.LoadScene("Initial Scene");

        if (Input.GetKeyDown(KeyCode.F))
            fpsPlayer.flashLight.SetActive(!fpsPlayer.flashLight.activeSelf);

        fpsPlayer.isShooting = Input.GetMouseButton(0);
    }

    public void LaunchHitEffect(Vector3 point, Vector3 direction)  // TODO: Zrušit vizuální efekt po přehrání
    {
        bulletExplosionEffect.SetVector3("position", point);
        bulletExplosionEffect.SetVector3("direction", direction);
        bulletExplosionEffect.SendEvent("OnStart");

        // GameObject.Find("dummy").transform.position = hit.point;  // only for debug
    }
}
