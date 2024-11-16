using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Ship;
using Random = UnityEngine.Random;

// To be attached to game controller

public class AiGeneral : MonoBehaviour
{
    public static AiGeneral aiGeneral;
    public List<Ship> shipsToBeSpawned;
    public List<Transform> spawnLocations;
    List<float> _shipsSizes = new();
    const float Spacing = 10;   // TODO: Get from ship size
    static SpawnData _spawnPassedData;

    void Start()
    {
        aiGeneral = this;

        foreach (Ship ship in shipsToBeSpawned)
            _shipsSizes.Add(ship.GetComponent<MeshFilter>().sharedMesh.bounds.extents.magnitude * ship.transform.localScale.x);
    }

   public IEnumerator SpawnEnemies(SpawnData spawnData)
    {
        yield return new WaitForSeconds(spawnData.DelayBeforeSpawn);

        Transform spawnLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];

        int sign = - 1;

        for (int i = 0; i < spawnData.ShipsCount; ++i)
        {
            // Vector3 pos = new(spawnLocation.position.x + GetSign() * i * Spacing, 0, spawnLocation.position.z + Random.Range(- Spacing, Spacing));
            Vector3 pos = new(spawnLocation.position.x + GetSign() * i * _shipsSizes[spawnData.ShipType] * 2, 0, spawnLocation.position.z + Random.Range(- 2, 2));

            aiGeneral.shipsToBeSpawned[spawnData.ShipType].transform.position = pos;
            aiGeneral.shipsToBeSpawned[spawnData.ShipType].transform.rotation = Quaternion.identity;
            aiGeneral.shipsToBeSpawned[spawnData.ShipType].transform.RotateAround(spawnLocation.position, Vector3.up, spawnLocation.eulerAngles.y);

            Ship newShip = Instantiate(aiGeneral.shipsToBeSpawned[spawnData.ShipType]/*, spawnLocation.transform, true*/);

            newShip.GetComponent<AiPilot>().IWantToAttack(ActiveShip);
        }

        int GetSign()
        {
            sign *= -1;
            return sign;
        }
    }

    public void SpawnSequence(List<SpawnData> spawnSequence)
    {
        float cumulativeDelay = 0;
        foreach (var spawnData in spawnSequence)  // Invoke all spawn data at once
        {
            cumulativeDelay += spawnData.DelayBeforeSpawn;
            spawnData.DelayBeforeSpawn = cumulativeDelay;   // Convert relative delays to absolute delays

            // aiGeneral.Invoke(nameof(SpawnEnemies), cumulativeDelay);
            StartCoroutine(SpawnEnemies(spawnData));

        }
    }

}
