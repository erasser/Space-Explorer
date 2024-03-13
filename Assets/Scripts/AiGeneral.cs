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
    const float Spacing = 10;   // TODO: Get from ship size
    static SpawnData _spawnPassedData;
    // TODO: Ještě celou formaci otočit podle spawnLocation (rotate around point?)

    void Start()
    {
        aiGeneral = this;
    }

   public IEnumerator SpawnEnemies(SpawnData spawnData)  // TODO: Random spawn location from list
    {
        yield return new WaitForSeconds(spawnData.DelayBeforeSpawn);

        int sign = - 1;

        for (int i = 0; i < spawnData.ShipsCount; ++i)
        {
            Vector3 pos = new(aiGeneral.spawnLocations[0].position.x + GetSign() * i * Spacing, 0, aiGeneral.spawnLocations[0].position.z + Random.Range(-Spacing, Spacing));
            var newShip = Instantiate(aiGeneral.shipsToBeSpawned[spawnData.ShipType], pos, aiGeneral.spawnLocations[0].rotation);
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
