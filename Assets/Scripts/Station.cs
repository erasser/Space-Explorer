using System.Collections.Generic;
using UnityEngine;
using static UniverseController;

public class Station : MonoBehaviour
{
    // [HideInInspector]
    // public bool dockable;
    [HideInInspector]
    public List<Transform> docks = new();
    [HideInInspector]
    public List<Transform> dockingStarts = new();
    const float scale = .3f;
    const float dockingStartOffset = 40;

    void Start()
    {
        foreach (Transform dockTransform in transform.Find("docks").transform)
        {
            // dockable = true;
            var dockableLights = Instantiate(Uc.dockingLightsPrefab, dockTransform);
            dockableLights.transform.localScale = new(
                Uc.dockingLightsPrefab.transform.localScale.x / transform.localScale.x * scale,
                Uc.dockingLightsPrefab.transform.localScale.y / transform.localScale.y * scale,
                Uc.dockingLightsPrefab.transform.localScale.z / transform.localScale.z * scale);
            dockableLights.transform.Translate(- 14 * scale * dockableLights.transform.lossyScale.x * dockTransform.right, Space.World);
            
            docks.Add(dockTransform);

            var dockingStart = new GameObject("docking start");
            dockingStart.transform.SetParent(dockTransform);
            dockingStart.transform.position = dockTransform.position;
            dockingStart.transform.Translate(- 28 * scale * dockableLights.transform.lossyScale.x * dockTransform.right, Space.World);
            
            dockingStarts.Add(dockingStart.transform);
        }
    }

}
