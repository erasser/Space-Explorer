using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using static FPSPlayer;

public class WorldController : MonoBehaviour
{
    public static WorldController Wc;
    public static Camera FPSCamera;
    public static Transform FPSCameraTransform;
    public static Vector2 ScreenHalfResolution;
    [SerializeField]
    public VisualEffect bulletExplosionEffectPrefab;
    [HideInInspector]
    public VisualEffect bulletExplosionEffect;
    public Transform tmpDummy;
    public LayerMask groundLayer;
    public static Text InfoText;

    void Awake()
    {
        Wc = this;
    }

    void Start()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        ScreenHalfResolution = new(Screen.width / 2, Screen.height / 2);
        bulletExplosionEffect = Instantiate(bulletExplosionEffectPrefab);
        InfoText = GameObject.Find("info text").GetComponent<Text>();
        FPSCamera = fpsPlayerTransform.Find("Joint/PlayerCamera").GetComponent<Camera>();
        FPSCamera = fpsPlayerTransform.Find("Joint/PlayerCamera").GetComponent<Camera>();
        FPSCameraTransform = FPSCamera.transform;
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

    public void LaunchHitEffect(Vector3 point, Vector3 direction)
    {
        bulletExplosionEffect.SetVector3("position", point);
        bulletExplosionEffect.SetVector3("direction", direction);
        bulletExplosionEffect.SendEvent("OnStart");
    }

    public static int GetRandomSign()
    {
        return Random.value < .5f ? - 1 : 1;
    }
}
