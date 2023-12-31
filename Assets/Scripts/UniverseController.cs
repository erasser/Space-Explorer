using System;
using System.Collections.Generic;
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
    // public LayerMask predictiveCollidersLayer;
    public static RaycastHit MouseCursorHit;
    public Camera mainCamera;
    public static Transform MainCameraTransform;
    public static Vector3 InitialCameraOffset;
    public GameObject astronautPrefab;
    public static Ship Astronaut;
    public Texture2D mouseCursor;
    GameObject _rangeSprite;
    // public GameObject selectionSpritePrefab;
    // [HideInInspector]
    // public GameObject selectionSprite;
    [SerializeField]
    public VisualEffect explosionEffectPrefab;
    [HideInInspector]
    public VisualEffect explosionEffect;
    float _initialFov;
    public static GameObject UI;
    public static Text InfoText;
    const float AstronautBoardingDistanceLimit = 4;
    static readonly float AstronautBoardingSqrDistanceLimit = Mathf.Pow(AstronautBoardingDistanceLimit, 2);
    [Tooltip("Will be cleared of disabled ships.")]
    public List<Ship> canBeBoardedList = new();
    // Transform _astronautRangeTransform;
    public GameObject predictPositionDummyPrefab;
    // public static Transform PredictPositionDummyTransform;
    public Caption captionPrefab;
    public float staticFixedUpdateDeltaTime = .2f;  // fot purpose of static calls (i.e. not called by each component as it's in FixedUpdate)
    float _lastStaticFixedUpdate;

    void Start()
    {
        universeController = this;
        MainCameraTransform = mainCamera.transform;
        _initialFov = mainCamera.fieldOfView;
        Astronaut = Instantiate(astronautPrefab).GetComponent<Ship>();
        Astronaut.gameObject.SetActive(false);
        Cursor.SetCursor(mouseCursor, new(mouseCursor.width / 2f, mouseCursor.height / 2f), CursorMode.ForceSoftware);
        explosionEffect = Instantiate(explosionEffectPrefab);
        UI = GameObject.Find("UI");
        InfoText = UI.transform.Find("infoText").GetComponent<Text>();
        canBeBoardedList.RemoveAll(ship => !ship.gameObject.activeSelf);
        // selectionSprite = Instantiate(selectionSpritePrefab);

        MyNavMeshAgent.CreatePredictiveCollider();
    }

    void Update()
    {
        ProcessKeys();

        ProcessMouseMove();

        ActiveShip.SetUserTarget(MouseCursorHit.point);

        UpdateCameraPosition();

        ProcessStaticFixedDeltaTime();
        MyNavMeshAgent.PredictCollisions();
        // _rangeSprite.transform.position = Astronaut.transformCached.position;
    }

    void ProcessStaticFixedDeltaTime()
    {
        if (Time.time > _lastStaticFixedUpdate + staticFixedUpdateDeltaTime)
        {
            StaticFixedUpdate();
            _lastStaticFixedUpdate = Time.time;
        }
    }

    static void StaticFixedUpdate()
    {
        // MyNavMeshAgent.PredictCollisions();
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

        if (Input.GetKey(KeyCode.LeftShift))
            ActiveShip.afterburnerCoefficient = 2;
        else
            ActiveShip.afterburnerCoefficient = 1;

        bool wasQPressedThisFrame = false;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            wasQPressedThisFrame = true;

            if (!IsAstronautActive() && !ActiveShip.isFiring)
                EjectAstronaut();
            else
                BoardAstronaut(ActiveShip.GetClosestShipInRange(AstronautBoardingDistanceLimit).ship);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (!wasQPressedThisFrame && IsAstronautActive())
            {
                var closestShip = Astronaut.GetClosestShipInRange();

                if (closestShip.ship)
                {
                    // var shipToAstronautV3 = closestShip.transformCached.position - Astronaut.transformCached.position;
                    // var shipToAstronautV3 =  closestShip.GetComponent<Collider>().ClosestPoint(Astronaut.transformCached.position) - Astronaut.transformCached.position;
                    var shipToAstronautV3 = closestShip.toClosestPointV3;
Debug.DrawRay(Astronaut.rb.position, SetVectorLength(shipToAstronautV3, 10), Color.cyan);
                    if (shipToAstronautV3.sqrMagnitude > AstronautBoardingSqrDistanceLimit)
                        Astronaut.rb.AddForce(60 * shipToAstronautV3.normalized, ForceMode.Impulse);  // TODO: Je to uvnitř Update()
                    // else
                    //     closestShip.ship.Highlight();
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
        // return;
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

        Astronaut.rb.AddForce(-10000 * ActiveShip.transformCached.forward, ForceMode.Impulse);
        ActiveShip.moveVector.x = ActiveShip.moveVector.z = 0;
        ActiveShip = Astronaut;

        mainCamera.fieldOfView = _initialFov / 2;
    }

    void ProcessMouseMove()
    {
        Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out MouseCursorHit, Mathf.Infinity, raycastPlaneLayer);
    }

    void UpdateCameraPosition()
    {
        var coef = Input.GetKey(KeyCode.LeftControl) ? .8f : .2f;

        MainCameraTransform.position = ActiveShip.transformCached.position +
                                        InitialCameraOffset + // vertical offset
                                        // SetVectorLength(player.toTargetV3, player.toTargetV3.sqrMagnitude / 3);  // horizontal offset  // Fungovalo to, teď problikává obraz
                                        ActiveShip.toTargetV3 * coef; // horizontal offset
    }

    void SetCameraHeight(float multiplier)
    {
        var startPos = MainCameraTransform.position;
        // var endPos = SetVectorLength(startPos, multiplier * startPos.y);
        var endPos = startPos - Vector3.up * 10;

        MainCameraTransform.gameObject.Tween("Zoom", startPos, endPos, 1.5f, TweenScaleFunctions.SineEaseInOut,
            Bubu);

        void Bubu(ITween<Vector3> t)
        {
            MainCameraTransform.position = t.CurrentValue;
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

    // Target's velocity is predicted, observer is checking collision / shooting with observerVelocity
    public static Vector3 GetPredictedPositionOffset(Ship target, Ship observer, float observerVelocity)  // https://gamedev.stackexchange.com/questions/25277/how-to-calculate-shot-angle-and-velocity-to-hit-a-moving-target
    {
        var targetVelocity = target.rb.velocity;

        if (targetVelocity == Vector3.zero)
            return Vector3.zero;

        Vector3 toTarget =  target.transformCached.position - observer.transformCached.position;
        float a = Vector3.Dot(targetVelocity, targetVelocity) - observerVelocity * observerVelocity;
        float b = 2 * Vector3.Dot(targetVelocity, toTarget);
        float c = Vector3.Dot(toTarget, toTarget);

        float p = - b / (2 * a);
        float q = Mathf.Sqrt(Mathf.Abs(b * b - 4 * a * c)) / (2 * a);

        float t1 = p - q;
        float t2 = p + q;
        float t;

        if (t1 > t2 && t2 > 0)
            t = t2;
        else
            t = t1;

        if (Double.IsNaN(t))
            return Vector3.zero;

        // Vector3 aimSpot = targetVelocity * Mathf.Abs(t);
        // Vector3 bulletPath = aimSpot - transform.position;
        //float timeToImpact = bulletPath.magnitude / bulletVelocity;//speed must be in units per second
        return targetVelocity * Mathf.Abs(t);
    }

}
