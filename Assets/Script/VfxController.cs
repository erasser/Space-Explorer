using UnityEngine;
using UnityEngine.VFX;
using static Ship;

public class VfxController : MonoBehaviour
{
    [SerializeField]
    VisualEffect visualEffect;

    void Start()
    {
        print(ActiveShip);

    }

    void Update()
    {
        visualEffect.SetFloat("speed", ActiveShip.rigidBody.velocity.magnitude);
    }
}
