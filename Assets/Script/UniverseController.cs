using DigitalRuby.Tween;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using static Ship;

public class UniverseController : MonoBehaviour
{
    public static UniverseController universeController;
    public LayerMask raycastPlaneLayer;
    public LayerMask shootableLayer;
    public LayerMask closestShipColliderLayer;  // all ships (my solution for multiple gameObject layers)
    public static RaycastHit MouseCursorHit;
    public Camera mainCamera;
    Transform _mainCameraTransform;
    Vector3 _initialCameraOffset;
    public GameObject astronautPrefab;
    public static Ship Astronaut;
    Rigidbody _astronautRb;
    public Texture2D mouseCursor;
    // public GameObject selectionSpritePrefab;
    // public GameObject selectionSprite;
    [SerializeField]
    public VisualEffect explosionEffectPrefab;
    public VisualEffect explosionEffect;
    float _initialFov;
    public static Text InfoText;
    const float AstronautBoardingDistanceLimit = 4;
    static readonly float AstronautBoardingSqrDistanceLimit = Mathf.Pow(AstronautBoardingDistanceLimit, 2);

    void Start()
    {
        universeController = this;
        _mainCameraTransform = mainCamera.transform;
        _initialCameraOffset = _mainCameraTransform.position - ActiveShip.transformCached.position;
        _initialFov = mainCamera.fieldOfView;
        Astronaut = Instantiate(astronautPrefab).GetComponent<Ship>();
        Astronaut.gameObject.SetActive(false);
        _astronautRb = Astronaut.GetComponent<Rigidbody>();
        Cursor.SetCursor(mouseCursor, new(32, 32), CursorMode.ForceSoftware);
        explosionEffect = Instantiate(explosionEffectPrefab);
        InfoText = GameObject.Find("infoText").GetComponent<Text>();
        // selectionSprite = Instantiate(selectionSpritePrefab);
    }

    void Update()
    {
        ProcessKeys();

        ProcessMouseMove();

        ActiveShip.SetUserTarget(MouseCursorHit.point);

        UpdateCameraPosition();
    }

    void ProcessKeys()
    {
        ActiveShip.isFiring = Input.GetMouseButton(0);

        if (Input.GetKey(KeyCode.W))
            ActiveShip.SetMoveVectorVertical(1);
        else if (Input.GetKey(KeyCode.S))
            ActiveShip.SetMoveVectorVertical(-1);
        else
            ActiveShip.SetMoveVectorVertical(0);

        if (Input.GetKey(KeyCode.A))
            ActiveShip.SetMoveVectorHorizontal(-1);
        else if (Input.GetKey(KeyCode.D))
            ActiveShip.SetMoveVectorHorizontal(1);
        else
            ActiveShip.SetMoveVectorHorizontal(0);

        bool wasQPressedThisFrame = false;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            wasQPressedThisFrame = true;

            if (!IsAstronautActive() && !ActiveShip.isFiring)
                EjectAstronaut();
            else
            {
                BoardAstronaut(ActiveShip.GetClosestShipInRange(AstronautBoardingDistanceLimit));
            }
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (!wasQPressedThisFrame && IsAstronautActive())
            {
                var closestShip = ActiveShip.GetClosestShipInRange();
// InfoText.text = closestShip.name;
                if (closestShip)
                {
                    var shipToAstronautV3 = closestShip.transformCached.position - Astronaut.transformCached.position;

                    if (shipToAstronautV3.sqrMagnitude > AstronautBoardingSqrDistanceLimit)
                        _astronautRb.AddForce(60 * shipToAstronautV3.normalized, ForceMode.Impulse);
                }
            }
        }
    }

    public void LaunchHitEffect(Vector3 point, Vector3 normal)
    {
        explosionEffect.SetVector3("position", point);
        explosionEffect.SetVector3("direction", normal);
        explosionEffect.SendEvent("OnStart");
    }

    void BoardAstronaut(Ship ship)
    {
        if (!ship)
            return;

        Astronaut.gameObject.SetActive(false);
        ActiveShip = ship;

        mainCamera.fieldOfView = _initialFov;
    }

    void EjectAstronaut()
    {
        Astronaut.gameObject.SetActive(true);
        Astronaut.transformCached.position = ActiveShip.transformCached.position;
        Astronaut.transformCached.rotation = ActiveShip.transformCached.rotation;

        Astronaut.GetComponent<Rigidbody>().AddForce(-10000 * ActiveShip.transformCached.forward, ForceMode.Impulse);
        ActiveShip.moveVector.x = ActiveShip.moveVector.z = 0;
        ActiveShip = Astronaut.GetComponent<Ship>();

        mainCamera.fieldOfView = 35;
    }

    void ProcessMouseMove()
    {
        Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out MouseCursorHit, Mathf.Infinity, raycastPlaneLayer);
    }

    void UpdateCameraPosition()
    {
        _mainCameraTransform.position = ActiveShip.transformCached.position +
                                        _initialCameraOffset + // vertical offset
                                        // SetVectorLength(player.toTargetV3, player.toTargetV3.sqrMagnitude / 3);  // horizontal offset  // Fungovalo to, teď problikává obraz
                                        ActiveShip.toTargetV3 * .2f; // horizontal offset
    }

    void SetCameraHeight(float multiplier)
    {
        var startPos = _mainCameraTransform.position;
        // var endPos = SetVectorLength(startPos, multiplier * startPos.y);
        var endPos = startPos - Vector3.up * 10;

        _mainCameraTransform.gameObject.Tween("Zoom", startPos, endPos, 1.5f, TweenScaleFunctions.SineEaseInOut,
            Bubu);

        void Bubu(ITween<Vector3> t)
        {
            _mainCameraTransform.position = t.CurrentValue;
        }
    }

    void ZoomIn()
    {
        var startFov = mainCamera.fieldOfView;
        var endFov = 40;

        // mainCamera.gameObject.Tween("ZoomIn", startFov, endFov, 100, TweenScaleFunctions.Linear, TweenFov);
        TweenFactory.Tween("ZoomIn", startFov, endFov, 100, TweenScaleFunctions.Linear, TweenFov);

    }

    void TweenFov(ITween<float> t)
    {
        print("tween start");
        mainCamera.fieldOfView = t.CurrentValue;
    }

    public static Vector3 SetVectorLength(Vector3 vector, float length)
    {
        return vector.normalized * length;
    }



}
