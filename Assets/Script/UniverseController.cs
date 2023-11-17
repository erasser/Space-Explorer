using DigitalRuby.Tween;
using UnityEngine;
using static Ship;

public class UniverseController : MonoBehaviour
{
    public LayerMask raycastPlane;
    RaycastHit _mouseCursorHit;
    public Camera mainCamera;
    Transform _mainCameraTransform;
    Vector3 _initialCameraOffset;
    public GameObject astronautPrefab;
    GameObject _astronaut;
    Rigidbody _astronautRb;
    public Texture2D mouseCursor;

    void Start()
    {
        _mainCameraTransform = mainCamera.transform;
        _initialCameraOffset = _mainCameraTransform.position - ActiveShip.transformCached.position;
        _astronaut = Instantiate(astronautPrefab);
        _astronautRb = _astronaut.GetComponent<Rigidbody>();
        Cursor.SetCursor(mouseCursor, new(32, 32), CursorMode.ForceSoftware);
    }

    void Update()
    {
        ProcessKeys();

        ProcessMouseMove();

        ActiveShip.SetUserTarget(_mouseCursorHit.point);

        UpdateCameraPosition();
    }

    void ProcessKeys()
    {
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

            if (!_astronaut.activeSelf)
                EjectAstronaut();
            else
            {
                if ((DefaultShip.transformCached.position - _astronaut.transform.position).sqrMagnitude <= 15)
                    BoardAstronaut();
            }
        }

        if (Input.GetKey(KeyCode.Q))
        {
            if (!wasQPressedThisFrame && _astronaut.activeSelf)
            {
                var shipToAstronautV3 = DefaultShip.transformCached.position - _astronaut.transform.position;

                if (shipToAstronautV3.sqrMagnitude > 15)
                    _astronautRb.AddForce(60 * shipToAstronautV3.normalized, ForceMode.Impulse);
                // else
                //     BoardAstronaut();
            }
        }

        if (Input.GetMouseButton(0))
        {
            print("lmb");
            ActiveShip.isFiring = true;
        }
        else
            ActiveShip.isFiring = false;
    }

    void BoardAstronaut()
    {
        _astronaut.SetActive(false);
        ActiveShip = DefaultShip;

        // SetCameraHeight(1);
        mainCamera.fieldOfView = 100;
    }

    void EjectAstronaut()
    {
        _astronaut.SetActive(true);
        _astronaut.transform.position = ActiveShip.transformCached.position;
        _astronaut.transform.rotation = ActiveShip.transformCached.rotation;

        _astronaut.GetComponent<Rigidbody>()
            .AddForce(-10000 * ActiveShip.transformCached.forward, ForceMode.Impulse);
        ActiveShip.moveVector.x = ActiveShip.moveVector.z = 0;
        ActiveShip = _astronaut.GetComponent<Ship>();

        // SetCameraHeight(.4f);
        mainCamera.fieldOfView = 40;
        // ZoomIn();
    }

    void ProcessMouseMove()
    {
        Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out _mouseCursorHit, Mathf.Infinity,
            raycastPlane);
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
