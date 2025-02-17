using System;
using System.Collections;
using System.Collections.Generic;
// using DigitalRuby.Tween;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;
using static Ship;
using static AiGeneral;
using Random = UnityEngine.Random;
using static MyMath;
using static Comet;

public class UniverseController : MonoBehaviour
{
    public static UniverseController Uc;
    [HideInInspector]
    public int shootablePlayerLayer;
    [HideInInspector]
    public int shootableEnvironmentLayer;
    [HideInInspector]
    public int shootableShipsNeutralLayer;
    [HideInInspector]
    public int shootableShipsEnemyLayer;
    public LayerMask closestShipColliderLayer;  // all ships (my solution for multiple gameObject layers)
    // public LayerMask predictiveCollidersLayer;
    public static Vector3 MouseCursorHitPoint;
    public Camera mainCamera;
    public static Camera MainCamera;
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
    [Space]
    [Tooltip("Ship the player begins with")]
    public Ship initialShip;
    [Tooltip("Will be cleared of disabled ships.")]
    public List<Ship> canBeBoardedList = new();
    [Space]
    // Transform _astronautRangeTransform;
    public Image predictPositionDummyPrefab;
    // public static Transform PredictPositionDummyTransform;
    public Caption captionPrefab;
    public float staticFixedUpdateDeltaTime = .2f;  // fot purpose of static calls (i.e. not called by each component as it's in FixedUpdate)
    float _lastStaticFixedUpdate;
    public BoxCollider predictiveColliderPrefab;
    public LayerMask predictiveColliderLayerMask;  // TODO: Bylo by hezký vytáhnout to z toho predictiveColliderPrefabu
    public GameObject dockingLightsPrefab;
    [HideInInspector]
    public Ship selectedObject;
    int _zoomLevel;
    // public Transform starMapBackgroundTransform;
    // Material _starMapMaterial;
    // public float starSpeed = .0001f;
    // Vector3 _initialCameraToBackgroundOffset;
    public Transform starFieldCameraTransform;
    public SS_Starfield2D starField;
    bool _warp;
    public static Plane RaycastPlaneY0 = new (Vector3.up, Vector3.zero);
    Vector3 _debugV3;
    Vector3 _lastCamPos;
    const float MaxCamTranslation = 20;
    float _camTranslationFuncParam; 
    float _camTranslationFuncParamSqr; 
    float _camTranslationFuncParamSqrHalf; 
    float _camTranslationFuncParamSqrQuarter;
    public Comet cometPrefab;
    public static bool ShipControlsEnabled = true;
    public GameObject grid;
    public Graph graph;

    void Awake()
    {
        Uc = this;
        shootablePlayerLayer = LayerMask.NameToLayer("shootablePlayer");
        shootableEnvironmentLayer = LayerMask.NameToLayer("shootableEnvironment");
        shootableShipsNeutralLayer = LayerMask.NameToLayer("shootableShipsNeutral");
        shootableShipsEnemyLayer = LayerMask.NameToLayer("shootableShipsEnemy");
    }

    void Start()
    {
        MainCamera = mainCamera;
        MainCameraTransform = MainCamera.transform;
        _initialFov = MainCamera.fieldOfView;
        Astronaut = Instantiate(astronautPrefab).GetComponent<Ship>();
        Astronaut.gameObject.SetActive(false);
        Cursor.SetCursor(mouseCursor, new(mouseCursor.width / 2f, mouseCursor.height / 2f), CursorMode.Auto);
        explosionEffect = Instantiate(explosionEffectPrefab);
        UI = GameObject.Find("UI dynamic");
        InfoText = UI.transform.Find("infoText").GetComponent<Text>();
        canBeBoardedList.RemoveAll(ship => !ship.gameObject.activeSelf);
        // selectionSprite = Instantiate(selectionSpritePrefab);
        MyNavMeshAgent.CreatePredictiveCollider();
        initialShip.SetAsActiveShip();  // TODO: Pokud není initialShip, nastavit astronauta?
        InitialCameraOffset = MainCameraTransform.position - ActiveShipTransform.position;
        // _starMapMaterial = starMapBackgroundTransform.GetComponent<Renderer>().material;
        // _initialCameraToBackgroundOffset = starMapBackgroundTransform.position - MainCameraTransform.position;
        _camTranslationFuncParam = 2 * Mathf.Sqrt(MaxCamTranslation);
        _camTranslationFuncParamSqr = Mathf.Pow(_camTranslationFuncParam, 2);
        _camTranslationFuncParamSqrHalf = _camTranslationFuncParamSqr / 2;
        _camTranslationFuncParamSqrQuarter = _camTranslationFuncParamSqrHalf / 2;
        StartCoroutine(CheckCreateComet());
    }

    void Update()
    {
        ProcessGeneralControls();

        ProcessShipControls();

        if (!ActiveShip.autopilot)
            ActiveShip.SetCustomTarget(MouseCursorHitPoint);

        UpdateCameraPosition();

        ProcessStaticFixedDeltaTime();
        // MyNavMeshAgent.PredictCollisions();

        // _rangeSprite.transform.position = Astronaut.transformCached.position;

        // UpdateBackgroundTexture();

        UpdateStarFieldCamera();
    }

    void UpdateMouseCursorHitPoint()
    {
        MouseCursorHitPoint = RaycastPlane(RaycastPlaneY0, MainCamera);
    }

    void UpdateBackgroundTexture()
    {
        // starMapBackgroundTransform.position = MainCameraTransform.position + _initialCameraToBackgroundOffset;
        // _starMapMaterial.mainTextureOffset = new Vector2(MainCameraTransform.position.x, MainCameraTransform.position.z) * starSpeed;
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

    void ProcessGeneralControls()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // List<SpawnData> testSequence1 = new();
            // testSequence1.Add(new SpawnData(0, 1));
            // testSequence1.Add(new SpawnData(0, 2, 5));
            // aiGeneral.SpawnSequence(testSequence1);

            List<SpawnData> testSequence2 = new();
            testSequence2.Add(new SpawnData(0, 3));
            // testSequence2.Add(new SpawnData(0, 2, 1));
            // testSequence2.Add(new SpawnData(0, 2, 2));
            // testSequence2.Add(new SpawnData(0, 4, 2));
            aiGeneral.SpawnSequence(testSequence2);
        }
        
        if (Input.GetKeyDown(KeyCode.Return))
            SceneController.LoadScene("FPS Scene");

        // if (Input.GetKeyDown(KeyCode.P))
        //     Time.timeScale = Time.timeScale > 0 ? 0 : 1;

        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleActiveShipAutopilot(!ActiveShip.autopilot);
            
            // ActiveShip.debugRotationEnabled = !ActiveShip.debugRotationEnabled;
            // ActiveShip.debugUhelPriStopu = ActiveShip.transform.eulerAngles.y;
            // print(ActiveShip.transform.eulerAngles.y);
        }
        
        if (Input.GetKeyDown(KeyCode.G))
            grid.SetActive(!grid.activeSelf);

        if (Input.GetKeyDown(KeyCode.Comma))
        {
            Time.timeScale /= 2;
            print("time scale: " + Time.timeScale);
        }

        if (Input.GetKeyDown(KeyCode.Period))
        {
            Time.timeScale *= 2;
            print("time scale: " + Time.timeScale);
        }
    }

    void ProcessShipControls()
    {
        if (!ShipControlsEnabled)
            return;

        ActiveShip.SetIsFiring(Input.GetMouseButton(0));

        if (Input.GetMouseButtonDown(1))
            SelectObject();

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
            ActiveShip.afterburnerCoefficient = 4;
        else
            ActiveShip.afterburnerCoefficient = 1;

        bool wasQPressedThisFrame = false;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            wasQPressedThisFrame = true;

            if (!IsAstronautActive() && !ActiveShip.isFiring)
                EjectAstronaut();
            else
                BoardAstronaut(ActiveShip.GetClosestShipInRange(canBeBoardedList, AstronautBoardingDistanceLimit).ship);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (!wasQPressedThisFrame && IsAstronautActive())
            {
                var closestShip = Astronaut.GetClosestShipInRange(canBeBoardedList);

                if (closestShip.ship)
                {
                    // var shipToAstronautV3 = closestShip.transformCached.position - Astronaut.transformCached.position;
                    // var shipToAstronautV3 =  closestShip.GetComponent<Collider>().ClosestPoint(Astronaut.transformCached.position) - Astronaut.transformCached.position;
                    var shipToAstronautV3 = closestShip.toClosestPointV3;
                    // Debug.DrawRay(Astronaut.rb.position, SetVectorLength(shipToAstronautV3, 10), Color.cyan);
                    if (shipToAstronautV3.sqrMagnitude > AstronautBoardingSqrDistanceLimit)
                        Astronaut.rb.AddForce(60 * shipToAstronautV3.normalized, ForceMode.Impulse);  // TODO: Je to uvnitř Update()
                    // else
                    //     closestShip.ship.Highlight();
                }
            }
        }
        //AdjustScrollWheelZoom(Input.mouseScrollDelta.y);
    }

    void AdjustScrollWheelZoom(float x)
    {
        _zoomLevel += (int) x;  // zoom value is inverted, so it doesn't have to be inverted again
        _zoomLevel = Mathf.Min(Mathf.Max(- 10, _zoomLevel), 10);
        // print(_zoomLevel);
        MainCameraTransform.Translate(InitialCameraOffset * _zoomLevel);
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
        ship.SetAsActiveShip();

        MainCamera.fieldOfView = _initialFov;
    }

    void EjectAstronaut()
    {
        Astronaut.gameObject.SetActive(true);
        Astronaut.transform.position = ActiveShipTransform.position;
        Astronaut.transform.rotation = ActiveShipTransform.rotation;

        Astronaut.rb.AddForce(- 10000 * ActiveShipTransform.forward, ForceMode.Impulse);
        ActiveShip.moveVector.x = ActiveShip.moveVector.z = 0;
        Astronaut.SetAsActiveShip();

        MainCamera.fieldOfView = _initialFov / 2;
    }

    void UpdateCameraPosition()
    {
        var newCamPos = ActiveShipTransform.position + InitialCameraOffset;
        newCamPos = new(newCamPos.x, InitialCameraOffset.y, newCamPos.z);
        MainCameraTransform.position = newCamPos;

        UpdateMouseCursorHitPoint();  // This have to be right here, so camera movement is smooth.

        var shipToCursor = SetVectorYToZero(MouseCursorHitPoint - ActiveShipTransform.position);

        var shipToCursorMagnitude = shipToCursor.magnitude;

        shipToCursor = shipToCursorMagnitude < _camTranslationFuncParamSqrHalf ?
            SetVectorLength(shipToCursor, shipToCursorMagnitude - Mathf.Pow(shipToCursorMagnitude / _camTranslationFuncParam, 2)) :
            SetVectorLength(shipToCursor, _camTranslationFuncParamSqrQuarter);
 
        var coef = Input.GetKey(KeyCode.LeftControl) ? 5f : 1f;
        var translateAmount = shipToCursor * coef;

        MainCameraTransform.Translate(translateAmount, Space.World);
    }

    /*void SetCameraHeight(float multiplier)
    {
        var startPos = MainCameraTransform.position;
        // var endPos = SetVectorLength(startPos, multiplier * startPos.y);
        var endPos = startPos - Vector3.up * 10;

        MainCameraTransform.gameObject.Tween("Zoom", startPos, endPos, 1.5f, TweenScaleFunctions.SineEaseInOut, Bubu);

        void Bubu(ITween<Vector3> t)
        {
            MainCameraTransform.position = t.CurrentValue;
        }
    }*/

    /*void ZoomIn()
    {
        var startFov = MainCamera.fieldOfView;
        var endFov = 40;

        // mainCamera.gameObject.Tween("ZoomIn", startFov, endFov, 100, TweenScaleFunctions.Linear, TweenFov);
        TweenFactory.Tween("ZoomIn", startFov, endFov, 100, TweenScaleFunctions.Linear, TweenFov);

    }*/

    /*void TweenFov(ITween<float> t)
    {
        print("tween start");
        MainCamera.fieldOfView = t.CurrentValue;
    }*/

    // Target's velocity is predicted, observer is checking collision / shooting with observerVelocity
    public static Vector3 GetPredictedPositionOffset(Ship target, Vector3 targetVelocity, GameObject observer, float observerSpeed)  // https://gamedev.stackexchange.com/questions/25277/how-to-calculate-shot-angle-and-velocity-to-hit-a-moving-target
    {
        // TODO: Chtěl bych ještě analyzovat výsledek a zahodit, pokud nedává smysl
  
        if (targetVelocity == Vector3.zero || observerSpeed == 0)
            return Vector3.zero;

        var toTarget =  target.transform.position - observer.transform.position;    
        
        float a = Vector3.Dot(targetVelocity, targetVelocity) - observerSpeed * observerSpeed;
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

        // TODO: Nastává toto vůbec? (Myslím, že s použitím VelocityExtimatoru ano) (Nastalo to po kolizi objektů)
        if (Double.IsNaN(t))
        {
            print("debug me ☺");
            return Vector3.zero;
        }

        // Vector3 aimSpot = targetVelocity * Mathf.Abs(t);
        // Vector3 bulletPath = aimSpot - transform.position;
        //float timeToImpact = bulletPath.magnitude / bulletVelocity;//speed must be in units per second
        return targetVelocity * Mathf.Abs(t);
    }

    public static Vector3 GetPredictedPositionOffset2(Ship target, Vector3 targetVelocity, Ship observer, float observerSpeed)  // moje řešení
    {
        var distanceVector = target.transform.position - observer.transform.position;
        var d = distanceVector.magnitude;
        var α = Vector3.Angle(distanceVector, targetVelocity);
        var cosα = Mathf.Cos(α);
        var voPerVtSquared = Mathf.Pow(observerSpeed / targetVelocity.magnitude, 2);

        var D = d * d * (cosα * cosα - 1 + voPerVtSquared);  // není zkutečný diskriminant, je to pokrácení dvojkou
        
        if (D < 0)
        {
            print("negative D, returning zero Vector");
            return Vector3.zero;
        }

        var Dsqrt = Mathf.Sqrt(D);

        var x1 = (d * cosα + Dsqrt) / (1 - voPerVtSquared);
        var x2 = (d * cosα - Dsqrt) / (1 - voPerVtSquared);
        
        print( "x1 = " + x1 + ", x2 = " + x2);
        float x;

        x = x1 >= 0 ? x1 : x2;
        
        if (Double.IsNaN(x))
        {
            print("I am NaN");
            return Vector3.zero;
        }

        return targetVelocity * x;
    }

    public static int GetRandomSign()
    {
        return Random.value < .5f ? - 1 : 1;
    }

    void SelectObject()
    {
        if (selectedObject)
            selectedObject.caption.gameObject.SetActive(false);

        if (Physics.Raycast(MainCamera.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, 1 << shootableShipsEnemyLayer | 1 << shootableShipsNeutralLayer))
        {
            selectedObject = hit.collider.gameObject.GetComponent<Ship>();
            selectedObject.caption.gameObject.SetActive(true);
        }
        else
            selectedObject = null;
    }

    void UpdateStarFieldCamera()
    {
        var pos = MainCameraTransform.position;
        starFieldCameraTransform.position = new(pos.x, pos.z, 0);
    }

    void UpdateWarpEffect()  // velocity 100 .. 400
    {
        // TODO: Lens distortion by se k tomu mohl přidat
        // _warp = enable;

        // if (enable)
        // {

        var c = Mathf.Max(ActiveShipVelocityEstimate.magnitude - 100, 0) / 300;
            
            starField.warp = .2f * c;
            starField.rotation = Vector2.Angle(
                new Vector2(ActiveShipVelocityEstimate.x, ActiveShipVelocityEstimate.z),
                Vector2.right) * Mathf.Deg2Rad;

            return;
        // }

        starField.warp = 0;
    }

    IEnumerator CheckCreateComet()
    {
        while (true)
        {
            if (FirstComet)
                yield return null;
            else
                yield return new WaitForSeconds(Random.Range(50f, 100f));  // TODO: V různých vesmírech by se range mohlo lišit => různý počet komet na různých místech

            CreateComet();            
        }
    }

    public void CreateComet()
    {
        Instantiate(cometPrefab);
    }

    public static void ToggleShipControls(bool enable)
    {
        ShipControlsEnabled = enable;

        if (!enable)
            ActiveShip.SetIsFiring(false);
    }
}
