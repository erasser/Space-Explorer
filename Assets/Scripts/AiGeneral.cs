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
    static List<SpawnData> actualSpawnSequence;
    static int actualSpawnSequenceIndex;
    // TODO: Ještě celou formaci otočit podle spawnLocation (rotate around point?)

    void Start()
    {
        aiGeneral = this;
    }

    public void SpawnEnemies()  // TODO: Random spawn location from list
    {
        int sign = - 1;

        for (int i = 0; i < actualSpawnSequence[actualSpawnSequenceIndex].ShipsCount; ++i)
        {
            Vector3 pos = new(aiGeneral.spawnLocations[0].position.x + GetSign() * i * Spacing, 0, aiGeneral.spawnLocations[0].position.z + Random.Range(-Spacing, Spacing));
            var newShip = Instantiate(aiGeneral.shipsToBeSpawned[actualSpawnSequence[actualSpawnSequenceIndex].ShipType], pos, aiGeneral.spawnLocations[0].rotation);
            newShip.GetComponent<AiPilot>().IWantToAttack(ActiveShip);
        }

        ++actualSpawnSequenceIndex;

        if (actualSpawnSequenceIndex == actualSpawnSequence.Count)
            return;

        print("invoking index: " + actualSpawnSequenceIndex);
        aiGeneral.Invoke(nameof(SpawnEnemies), actualSpawnSequence[actualSpawnSequenceIndex].DelayBeforeSpawn);

        int GetSign()
        {
            sign *= -1;
            return sign;
        }
    }

    public void SpawnSequence(List<SpawnData> spawnSequence)
    {
            print("initiating spawn list");
            // Task.Delay(500).ContinueWith(task => SpawnEnemies(spawnSequence[0]));

            actualSpawnSequence = spawnSequence;
            actualSpawnSequenceIndex = 0;
            // aiGeneral.Invoke(nameof(SpawnEnemies), spawnSequence[0].DelayBeforeSpawn);
            aiGeneral.Invoke(nameof(SpawnEnemies), 2);
    }

}
