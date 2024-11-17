using UnityEngine;
using static FPSPlayer;

public class WorldController : MonoBehaviour
{
    void Start()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
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
}
