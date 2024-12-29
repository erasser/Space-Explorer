using System.Collections.Generic;
using UnityEngine;
using static FPSPlayer;

// TODO: Terrainy mimo dosah disablovat

public class FPSTerrainController : MonoBehaviour
{
    public static FPSTerrainController fpsTerrainController;
    static float _length;
    static float _halfLength;
    public static List<FPSTerrain> Terrains = new();
    static Vector2 _lastPlayerCoordinates;
    static List<Vector2> _toBeVisibleTerrainsCoordinates = new();
    public static readonly Dictionary<Vector2, int> CoordsToIndex = new();
    public FPSTerrain terrainPrefab;
    static FPSTerrain _terrainPrefabStatic;
    public List<GameObject> _rocksPrefabs;

    public Material debugMaterial;
    Material _debugOriginalMat;

    void Start()
    {
        fpsTerrainController = this;
        _terrainPrefabStatic = terrainPrefab;
        _lastPlayerCoordinates = new(Mathf.Infinity, 0);  // To force terrain creation on init
        _length = terrainPrefab.GetComponent<Renderer>().bounds.extents.x * 2;
        _halfLength = _length / 2;

        // GenerateRocks(Terrains[0].transform);

        _debugOriginalMat = terrainPrefab.GetComponent<Renderer>().sharedMaterial;
    }

    public void CheckTerrains()
    {
        var position = fpsPlayerTransform.position;
        var coords = WorldPositionToTerrainCoordinates(position.x, position.z);

        if (_lastPlayerCoordinates == coords)
            return;

        _lastPlayerCoordinates = coords;

        UpdateToBeVisibleTerrainsCoordinatesList(coords);

        CreateTerrains();

        print("Terrains:");
        foreach (FPSTerrain t in Terrains)
        {
            print(t.xCoordinate + " , " + t.zCoordinate);
        }

        ClearAfterCreation();
    }

    void CreateTerrains()
    {
        foreach (Vector2 coord in _toBeVisibleTerrainsCoordinates)
        {
            var terrain = GetTerrainFromCoordinates(coord);
            
            if (!terrain)
            {
                CreateTerrainAtCoords(coord);
                // print(coord + " creating");
            }
            else if (!terrain.gameObject.activeSelf)
            {
                terrain.gameObject.SetActive(true);
                // print("Setting active: " + terrain.xCoordinate + " , " + terrain.zCoordinate + " , coord: " + coord);

            }
            // else
            //     print("Is active: " + terrain.xCoordinate + " , " + terrain.zCoordinate + ", coord: " + coord);

            // else if (terrain.GetComponent<Renderer>().material == debugMaterial)
                // terrain.GetComponent<Renderer>().material = _debugOriginalMat;
        }
    }

    static void ClearAfterCreation()
    {
        foreach (FPSTerrain terrain in Terrains)
        {
            bool needsToBeVisible = false;

            foreach (Vector2 coord in _toBeVisibleTerrainsCoordinates)
                if (coord.x == terrain.xCoordinate && coord.y == terrain.zCoordinate)
                    needsToBeVisible = true;

            if (!needsToBeVisible)
                terrain.gameObject.SetActive(false);
                // terrain.gameObject.GetComponent<Renderer>().material = fpsTerrainController.debugMaterial;
        }
    }

    static void UpdateToBeVisibleTerrainsCoordinatesList(Vector2 coord)
    {
        _toBeVisibleTerrainsCoordinates.Clear();
        
        _toBeVisibleTerrainsCoordinates.Add(new(coord.x - 1, coord.y - 1));
        _toBeVisibleTerrainsCoordinates.Add(new(coord.x, coord.y - 1));
        _toBeVisibleTerrainsCoordinates.Add(new(coord.x + 1, coord.y - 1));

        _toBeVisibleTerrainsCoordinates.Add(new(coord.x - 1, coord.y));
        _toBeVisibleTerrainsCoordinates.Add(new(coord.x, coord.y));
        _toBeVisibleTerrainsCoordinates.Add(new(coord.x + 1, coord.y));

        _toBeVisibleTerrainsCoordinates.Add(new(coord.x - 1, coord.y + 1));
        _toBeVisibleTerrainsCoordinates.Add(new(coord.x, coord.y + 1));
        _toBeVisibleTerrainsCoordinates.Add(new(coord.x + 1, coord.y + 1));
        
        // print("--------------  actual coord: " + coord);
        // foreach (Vector2 v in _toBeVisibleTerrainsCoordinates)
        //     print(v + "\n");
    }

    static Vector2 WorldPositionToTerrainCoordinates(float posX, float posY)
    {
        var signX = posX >= 0 ? 1 : -1;
        var signY = posY >= 0 ? 1 : -1;

        var x = (int)((posX + signX * _halfLength) / _length);
        var y = (int)((posY + signY * _halfLength) / _length);

        return new(x, y);
    }

    static Vector2 TerrainCoordinatesToWorldPosition(Vector2 coord)
    {
        return coord * _length;
    }

    static FPSTerrain GetTerrainFromCoordinates(Vector2 coords)
    {
        if (CoordsToIndex.ContainsKey(coords))
            return Terrains[CoordsToIndex[coords]];

        return null;
    }

    void CreateTerrainAtCoords(Vector2 coord)
    {
        var terrain = Instantiate(_terrainPrefabStatic);
        Vector2 pos = TerrainCoordinatesToWorldPosition(coord);

        var terrainTransform = terrain.transform;
        terrainTransform.position = new(pos.x, 0, pos.y);
        terrain.xCoordinate = (int)coord.x;
        terrain.zCoordinate = (int)coord.y;

        var scaleX = terrainTransform.localScale.x * (terrain.xCoordinate % 2 == 0 ? 1 : -1);
        var scaleZ = terrainTransform.localScale.z * (terrain.zCoordinate % 2 == 0 ? 1 : -1);

        terrainTransform.localScale = new(scaleX, terrainTransform.localScale.y, scaleZ);

        // GenerateRocks(terrainTransform);
        
        // ------------------ DEBUG ------------------
        var dummy = Instantiate(_rocksPrefabs[0]);
        dummy.transform.position = terrainTransform.position;
        dummy.transform.localScale = Vector3.one * 10;
    }

    void GenerateRocks(Transform terrainTransform)
    {
        var scaleMin = .02f;
        var scaleMax = .1f;

        foreach (GameObject rockPrefab in _rocksPrefabs)
            for (int i = 0; i < 200; ++i)
            {
                var rock = Instantiate(rockPrefab, terrainTransform);
                var tr = rock.transform;
                var terrainPosition = terrainTransform.position;
                var xPos = Random.Range(- _halfLength, _halfLength) + terrainPosition.x;
                var zPos = Random.Range(- _halfLength, _halfLength) + terrainPosition.z;

                tr.rotation = Random.rotation;

                var scale = Random.Range(scaleMin, scaleMax);
                tr.localScale = new(scale, scale, scale);

                // tr.position = new(xPos, GetYPosition(xPos, zPos, rock.GetComponent<Renderer>().bounds.extents.y / 2), zPos);
                tr.position = new(xPos, GetYPosition(xPos, zPos, rock.GetComponent<Renderer>().bounds.extents.y / 2), zPos);
                
                
                //--------
                var rock2 = Instantiate(rockPrefab, terrainTransform);
                rock2.transform.position = new(xPos, GetYPosition2(xPos, zPos), zPos);
                rock2.transform.localScale = Vector3.one * .05f;
            }

        float GetYPosition(float x, float z, float halfYExtent)
        {
            return 10;
            Physics.Raycast(new(x, 1000, z), Vector3.down, out var hit);

            return hit.point.y + halfYExtent;
        }
        float GetYPosition2(float x, float z)
        {
            Physics.Raycast(new(x, 1000, z), Vector3.down, out var hit);

            return hit.point.y /*+ halfYExtent*/;
        }
    }
}
