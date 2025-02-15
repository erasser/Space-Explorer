using UnityEngine;
using UnityEngine.UI;

public class ShipForces : MonoBehaviour
{
    Rigidbody _rb;
    Text _infoText;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _infoText = GameObject.Find("info text 1").GetComponent<Text>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            _rb.AddForce(100 * transform.forward, ForceMode.Acceleration);

        _infoText.text = _rb.velocity.magnitude.ToString();
    }
}
