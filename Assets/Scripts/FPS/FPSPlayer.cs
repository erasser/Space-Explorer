using UnityEngine;

public class FPSPlayer : MonoBehaviour
{
    public static FPSPlayer fpsPlayer;
    public GameObject flashLight;
    public bool isShooting;

    void Start()
    {
        fpsPlayer = this;
    }

    void Update()
    {
        
    }

    void ProcessShooting()
    {
        if (!isShooting)
            return;
    }
}
