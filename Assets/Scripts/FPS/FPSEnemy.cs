using UnityEngine;
using UnityEngine.AI;
using static FPSPlayer;

public class FPSEnemy : MonoBehaviour
{
    NavMeshAgent _agent;
    State _state;
    Vector3 _targetLocation;

    enum State
    {
        StandingStill,
        RandomRoaming,
        FollowingTarget
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        SetStateRandomRoamingNewDestination();
        // _agent.SetDestination(fpsPlayer.transform.position);
    }

    void Update()
    {
        ProcessStates();
    }

    void ProcessStates()
    {
        if (_state == State.StandingStill)
            return;

        if (_state == State.RandomRoaming)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                // if (!_agent.hasPath || _agent.velocity.sqrMagnitude == 0f)
                // {
                SetStateRandomRoamingNewDestination();
                // }
            }
        }

        if (_state == State.FollowingTarget)
        {
            var toTarget = fpsPlayer.transform.position - transform.position;
        }
    }

    void SetStateRandomRoamingNewDestination()
    {
        var a = 150;
        _state = State.RandomRoaming;
        var result = _agent.SetDestination(new(Random.Range(-a, a), 0, Random.Range(-a, a)));

        if (!result)
        {
            print("• Destination not set!");
            _agent.SetDestination(Vector3.zero);
        }
    }
}
