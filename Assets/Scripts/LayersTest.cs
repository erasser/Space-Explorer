using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayersTest : MonoBehaviour
{
    public LayerMask firstLayerMask;
    public LayerMask secondLayerMask;
    // public Layer firstLayer;
    // public Layer secondLayer;

    void Start()
    {
        int layer1 = LayerMask.NameToLayer("test1");
        int layer2 = LayerMask.NameToLayer("test2");

        var layerMask1 = 1 << layer1;
        var layerMask2 = 1 << layer2;

        var finalMask = layerMask1 | layerMask2;

        RaycastHit[] hits;
        hits = Physics.RaycastAll(Vector3.zero, Vector3.forward, Mathf.Infinity, finalMask);

        foreach (RaycastHit hit in hits)
        {
            print("hit: " + hit.collider.name);
        }
    }
}
