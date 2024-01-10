using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MyNavMeshAgent))]
public class AiPilot : CachedMonoBehaviour
{
    MyNavMeshAgent _myNavMeshAgent;
    TravelStance _travelStance;

    // enum Will
    // {
    //     StandStill,
    //     Patrol
    // }


    // What to do when enemy is engaged when travelling
    public enum TravelStance
    {
        PreferDestination,  // Ignore enemy and continue to destination
        PreferAttackEnemy,  // Attack enemy if in range, destroy him and continue to destination
        FollowEnemy         // Stand still when enemy is destroyed
    }

    void Start()
    {
        _myNavMeshAgent = GetComponent<MyNavMeshAgent>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            GoToRandomLocation();
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (MyNavMeshAgent.MyNavMeshAgents[0] == _myNavMeshAgent)
                IWantToPatrolTo(new(100, 0, 0));
            else
                IWantToPatrolTo(new(0, 0, 100));
        }
    }

    public void GoToRandomLocation()
    {
        var randomDestination = new Vector3(Random.Range(-300, 300), 0, Random.Range(-300, 300));
        IWantToGoTo(randomDestination);
        
        var dummy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dummy.transform.position = randomDestination;
        dummy.GetComponent<Collider>().isTrigger = true;
    }

    void IWantToStandStill()
    {
        _myNavMeshAgent.Stop();
    }

    void IWantToGoTo(Vector3 destination)
    {
        if (!_myNavMeshAgent.SetDestination(destination))
        {
            Debug.LogWarning("• NO VALID PATH FOUND!");
            return;
        }
        _travelStance = TravelStance.PreferDestination;
    }

    void IWantToPatrolTo(Vector3 secondPosition /*, TravelStance travelStance = TravelStance.PreferDestination*/)
    {
        _myNavMeshAgent.SetPatrolTo(secondPosition);
    }
}
