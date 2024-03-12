using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    // public List<SingleSpawn> SpawnSequence;
    // TODO: Ještě celou formaci otočit podle spawnLocation (rotate around point?)

    void Start()
    {
        aiGeneral = this;
    }

    IEnumerator SpawnEnemies(SpawnData spawnData)  // TODO: Random spawn location from list
    {
        yield return new WaitForSeconds(spawnData.DelayBeforeSpawn);

        print(spawnData);
        
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
        // foreach (SpawnData spawnData in spawnSequence)
        // {
            print("initiating delay");
            // Task.Delay(500).ContinueWith(task => SpawnEnemies(spawnSequence[0]));
            // aiGeneral.Invoke(nameof(test), 10);
            
            StartCoroutine(SpawnEnemies(spawnSequence[0]));

            // }
    }
    
    // IEnumerator MyFunction(bool status, float delayTime)
    // {
    //     yield return new WaitForSeconds(delayTime);
    //     print("hovno");
    // }
    //
    // public static void test(float a)
    // {
    //     print("huh " + a);
    // }
}
