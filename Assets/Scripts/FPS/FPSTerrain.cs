using UnityEngine;
using static FPSTerrainController;

public class FPSTerrain : MonoBehaviour
{
    public int xCoordinate;
    public int zCoordinate;

    void Awake()
    {
        Terrains.Add(this);
    }

    void Start()
    {
        CoordsToIndex.Add(new(xCoordinate, zCoordinate), Terrains.Count - 1);
    }
}
