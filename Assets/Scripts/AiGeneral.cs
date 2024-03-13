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
    static List<SpawnData> _actualSpawnSequence;
    static List<List<SpawnData>> _listSpawnSequences = new();
    static int _actualSpawnSequenceIndex;
    // TODO: Ještě celou formaci otočit podle spawnLocation (rotate around point?)

    void Start()
    {
        aiGeneral = this;
    }

    public void SpawnEnemies()  // TODO: Random spawn location from list
    {
        int sign = - 1;

        for (int i = 0; i < _actualSpawnSequence[_actualSpawnSequenceIndex].ShipsCount; ++i)
        {
            Vector3 pos = new(aiGeneral.spawnLocations[0].position.x + GetSign() * i * Spacing, 0, aiGeneral.spawnLocations[0].position.z + Random.Range(-Spacing, Spacing));
            var newShip = Instantiate(aiGeneral.shipsToBeSpawned[_actualSpawnSequence[_actualSpawnSequenceIndex].ShipType], pos, aiGeneral.spawnLocations[0].rotation);
            newShip.GetComponent<AiPilot>().IWantToAttack(ActiveShip);
        }

        ++_actualSpawnSequenceIndex;

        if (_actualSpawnSequenceIndex == _actualSpawnSequence.Count)
            return;

        print("invoking index: " + _actualSpawnSequenceIndex);
        aiGeneral.Invoke(nameof(SpawnEnemies), _actualSpawnSequence[_actualSpawnSequenceIndex].DelayBeforeSpawn);

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

            _actualSpawnSequence = spawnSequence;
            _actualSpawnSequenceIndex = 0;
            // aiGeneral.Invoke(nameof(SpawnEnemies), spawnSequence[0].DelayBeforeSpawn);
            aiGeneral.Invoke(nameof(SpawnEnemies), 2);
    }

}
