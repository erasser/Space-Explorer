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

    void Start()
    {
        _mainCameraTransform = mainCamera.transform;
        _initialCameraOffset = _mainCameraTransform.position - ActiveShip.transformCached.position;
        _astronaut = Instantiate(astronautPrefab);
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
        
        if (Input.GetKey(KeyCode.Q))
        {
            if (!_astronaut.activeSelf)
            {
                _astronaut.SetActive(true);
                _astronaut.transform.position = ActiveShip.transformCached.position;
                _astronaut.transform.rotation = ActiveShip.transformCached.rotation;
                _astronaut.GetComponent<Rigidbody>().AddForce(- 10000 * _astronaut.transform.forward, ForceMode.Impulse);
            }
            else
            {
                _astronaut.SetActive(false);
            }
        }
    }

    void ProcessMouseMove()
    {
        Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out _mouseCursorHit, Mathf.Infinity, raycastPlane);
    }

    void UpdateCameraPosition()
    {
        _mainCameraTransform.position = ActiveShip.transformCached.position + _initialCameraOffset +                      // vertical offset
                                        // SetVectorLength(player.toTargetV3, player.toTargetV3.sqrMagnitude / 3);  // horizontal offset  // Fungovalo to, teď problikává obraz
                                        ActiveShip.toTargetV3 * .2f;  // horizontal offset
    }

    
    

    public static Vector3 SetVectorLength(Vector3 vector, float length)
    {
        return vector.normalized * length;
    }
    
}
