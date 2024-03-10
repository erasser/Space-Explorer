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
    // TODO: Ještě celou formaci otočit podle spawnLocation (rotate around point?)

    void Start()
    {
        aiGeneral = this;
    }

    public static void SpawnEnemies(int type, int count)  // TODO: Random spawn location from list
    {
        int sign = - 1;

        for (int i = 0; i < count; ++i)
        {
            Vector3 pos = new(aiGeneral.spawnLocations[0].position.x + GetSign() * i * Spacing, 0, aiGeneral.spawnLocations[0].position.z + Random.Range(-Spacing, Spacing));
            var newShip = Instantiate(aiGeneral.shipsToBeSpawned[type], pos, aiGeneral.spawnLocations[0].rotation);
            newShip.GetComponent<AiPilot>().IWantToAttack(ActiveShip);
        }

        int GetSign()
        {
            sign *= -1;
            return sign;
        }
    }


}
