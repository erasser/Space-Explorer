using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MyNavMeshAgent))]
public class AiPilot : MonoBehaviour
{
    MyNavMeshAgent _myNavMeshAgent;
    // TravelStance _travelStance;
    // const float _dockingTime = 10;
    // bool _docked;
    // float _dockedAt;
    // Dictionary<DockingPurpose, float> _dockingTimes = new ();
    // public DockingPurpose dockingPurpose;

    // public enum DockingPurpose
    // {
    //     JustChilling,
    //     UnloadingOre
    // }

    // enum Will
    // {
    //     StandStill,
    //     Patrol
    // }

    // What to do when enemy is engaged when travelling
    // public enum TravelStance
    // {
    //     PreferDestination,  // Ignore enemy and continue to destination
    //     PreferAttackEnemy,  // Attack enemy if in range, destroy him and continue to destination
    //     FollowEnemy         // Stand still when enemy is destroyed
    // }

    void Awake()
    {
        _myNavMeshAgent = GetComponent<MyNavMeshAgent>();

        // _dockingTimes.Add(DockingPurpose.JustChilling, 30);
        // _dockingTimes.Add(DockingPurpose.UnloadingOre, 10);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            GoToRandomLocation();
        if (Input.GetKeyDown(KeyCode.F))
            IWantToAttack(Ship.ActiveShip);
        if (Input.GetKeyDown(KeyCode.P))
        {
            // if (MyNavMeshAgent.MyNavMeshAgents[0] == _myNavMeshAgent)
            if (_myNavMeshAgent.transform.position.z == -50)  // To bylo k testování collision prediction
                IWantToPatrolTo(new(-100, 0, -75));
            else
                IWantToPatrolTo(new(-100, 0, -75));
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            _myNavMeshAgent.ReGeneratePath();

            _myNavMeshAgent.CheckFollowingEnemyDistance();
        }

        // if (Input.GetKeyDown(KeyCode.M))
        //     IWantToMine();
    }

    public void GoToRandomLocation()
    {
        var randomDestination = new Vector3(Random.Range(-300, 300), 0, Random.Range(-300, 300));
        IWantToGoTo(randomDestination);
        
        var dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dummy.transform.position = randomDestination;
        dummy.GetComponent<Collider>().isTrigger = true;
    }

    public void IWantToStandStill()
    {
        _myNavMeshAgent.Stop();
    }

    public void IWantToGoTo(Vector3 destination)
    {
        if (!_myNavMeshAgent.SetDestination(destination))
        {
            Debug.LogWarning("• NO VALID PATH FOUND!");
            return;
        }
        // _travelStance = TravelStance.PreferDestination;
    }

    public void IWantToPatrolTo(Vector3 secondPosition /*, TravelStance travelStance = TravelStance.PreferDestination*/)
    {
        _myNavMeshAgent.SetPatrolTo(secondPosition);
    }

    public void IWantToAttack(Ship ship)
    {
        _myNavMeshAgent.SetTargetToFollow(ship);
    }

    /*void IWantToMine()
    {
        var miningAsteroid = GetNearestMineableAsteroid();
        if (!miningAsteroid)
        {
            print("No asteroid to mine.");
            return;
        }

    }

    GameObject GetNearestMineableAsteroid()
    {
        return GameObject.Find("asteroid");
    }*/
}
