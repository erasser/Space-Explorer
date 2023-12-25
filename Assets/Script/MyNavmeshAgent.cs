using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static Ship;

// TODO: Patrol
// TODO: Zajistit, aby byla cesta korektní po kolizi (tj. změny pozice) objektu.
// ↑ Funguje to, ale asi jen do chvíle, než objekt dotrkám k dalšímu checkpoinu a on se bude snažit vrátit k tomu aktuálnímu.
// ↑ Asi by to v případě kolize chtělo najít nejbližší checkpoint. TODO 

[RequireComponent(typeof(Rigidbody), typeof(LineRenderer), typeof(AiPilot))]
public class MyNavMeshAgent : CachedMonoBehaviour
{
    public bool showPath = true;
    public float targetMinDistance = 2f;
    public float agentFixedUpdateDeltaTime = .2f;
    float _lastAgentFixedUpdate;
    Ship _ship;
    NavMeshPath _navMeshPath;
    readonly List<Vector3> _pathPoints = new();
    int _actualPathPointIndex;
    float _targetSqrMinDistance;
    LineRenderer _lineRenderer;
    GameObject _pointVisualizer;
    Vector3 _toActualPathPointDirection;
    State _state;
    // TARGETS and DESTINATIONS  (helper class to be created)
    Ship _target;
    readonly List<Vector3> _destinations = new(new Vector3[2]);  // TODO: Just two for now
    int _actualDestinationIndex;

    // public ReportedEvent reportedEvent; 

    // public enum ReportedEvent
    // {
    //     NothingToReport,
    //     TargetReached
    // }

    enum State
    {
        StandingStill,
        GoingToDestination,
        Patrolling,
        FollowingEnemy
    }

    void Start()
    {
        _ship = GetComponent<Ship>();
        _lineRenderer = GetComponent<LineRenderer>();
        _navMeshPath = new();
        _lastAgentFixedUpdate = Time.time;
        _targetSqrMinDistance = Mathf.Pow(targetMinDistance, 2);
        // _aiPilot = GetComponent<AiPilot>();
    }

    void Update()
    {
        if (Time.time > _lastAgentFixedUpdate + agentFixedUpdateDeltaTime)
        {
            AgentFixedUpdate();
            _lastAgentFixedUpdate = Time.time;
        }

        if (_state == State.GoingToDestination)
        {
            _ship.SetMoveVectorHorizontal(_toActualPathPointDirection.x);
            _ship.SetMoveVectorVertical(_toActualPathPointDirection.z);
        }
    }

    void AgentFixedUpdate()
    {
        CheckPathPoints();

        if (_state == State.GoingToDestination)
        {
            if (!GeneratePathTo(_destinations[0]))
            {
                Debug.LogWarning("◘ NO VALID PATH FOUND! "); // TODO: Najednou se nejde dostat do cíle
                return;
            }
            
            // TODO: Může to najít jen partial
            
            
        }

        return;
        
        /*// if (_state == State.GoingToDestination)
        if (rigidBodyCached.velocity.sqrMagnitude > 0)
        {
            CheckEnemies();

            // Tohle by mělo probíhat spíš v podmínce velocity.magnitude > 0
            if (IsPathPointReached())
            {
                ++_actualPathPointIndex;

                if (_actualPathPointIndex == _pathPoints.Count)  // last point
                    Stop();
            }
        }
        else if (_state == State.FollowingEnemy)
        {
            GeneratePathTo(_target.transformCached.position);
        }*/
    }

    // TODO: This will go to AI to manage
    // void CheckEnemies()  // TODO: player is the only enemy now
    // {
    //     if (IsEnemyInRange() /*&& !_ship.isFiring*/) // Enemy is in range and the ship isn't firing yet
    //     {
    //         _ship.isFiring = true;               // Cease fire
    //
    //         if (_travelStance == TravelStance.PreferAttackEnemy || _travelStance == TravelStance.FollowEnemy)
    //         {
    //             SetTargetToFollow();  // potřebuju toho novýho enemyho + přepodmínkovat
    //         }
    //     }
    //     else
    //     {
    //         /*if (_ship.isFiring)*/
    //         _ship.isFiring = false;
    //     }
    // }

    void CheckPathPoints()
    {
        if (_actualPathPointIndex >= _pathPoints.Count)
        {
            print("☺ Key doesn't exist, returning");
            return;
        }

        _toActualPathPointDirection = _pathPoints[_actualPathPointIndex] - transformCached.position;
        var distance = _actualPathPointIndex == _pathPoints.Count - 1 ? _targetSqrMinDistance * 1.5f : _targetSqrMinDistance;  // Break sooner before last path point
        var reached = _toActualPathPointDirection.sqrMagnitude <= distance;

        print(_toActualPathPointDirection.sqrMagnitude + ", " + distance);
        
        if (reached)
        {
            print("• reached!");
            ++_actualPathPointIndex;
            _pathPoints.Clear();
            _actualPathPointIndex = -1;

            if (_actualPathPointIndex == _pathPoints.Count) // last point
                Stop();
        }
    }

    bool GeneratePathTo(Vector3 targetLocation)
    {
        CheckPathPoints();

        if (_actualPathPointIndex == -1)
        {
            print("• _actualPathPointIndex = -1");
            return false;
        }

        var position = transformCached.position;
        var result = NavMesh.CalculatePath(new(position.x, 0, position.z), targetLocation, NavMesh.AllAreas, _navMeshPath);

        if (!result)
            return false;

        _actualPathPointIndex = 0;
        _pathPoints.Clear();

        var count = _navMeshPath.corners.Length;        
        for (var i = 1; i < count; ++i)
            _pathPoints.Add(_navMeshPath.corners[i]);

        _state = State.GoingToDestination;

        ShowPath();

        return true;
    }

    void ShowPath()
    {
        if (!showPath)
            return;

        _lineRenderer.positionCount = _navMeshPath.corners.Length;
        _lineRenderer.SetPositions(_navMeshPath.corners);

        // foreach (var point in _navMeshPath.corners)
        // {
        //     _pointVisualizer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        //     _pointVisualizer.transform.position = point;
        //     _pointVisualizer.name = $"{name}_path_point";
        // }
    }

    public bool SetDestination(Vector3 target /*, TravelStance travelStance = TravelStance.PreferDestination*/)
    {
        if (!GeneratePathTo(target))
            return false;
print("_destinations capacity: " + _destinations.Capacity);
print("_destinations count: " + _destinations.Count);
        _destinations[0] = target;
        _state = State.GoingToDestination;

        return true;
    }

    /*public void SetTargetToFollow(Ship target)
    {
        _target = target;
        _state = State.GoingToDestination;
        _travelStance = TravelStance.FollowEnemy;

        // GeneratePathTo(target.transformCached.position);  // bude se dít v updatu
    }*/

    public void Stop()
    {
        _state = State.StandingStill;
        _ship.SetMoveVectorHorizontal(0);
        _ship.SetMoveVectorVertical(0);
    }

    bool IsEnemyInRange()
    {
        var sqrDistance = (ActiveShip.transformCached.position - transformCached.position).sqrMagnitude;
        return sqrDistance <= ShootingSqrRange;
    }

    public void Patrol(Vector3 destinationPoint)
    {
        // TODO: Checknout situaci, kdy 2. bod je už nadosah

        _destinations[0] = transformCached.position;
        _destinations[1] = destinationPoint;
        
        // ...        

    }

    public void PauseMotion()
    {
        
    }

    public void ResumeMotion()
    {
        
    }
    
    
    // TODO: Nějak low-level zajistit, aby cíl měl vždy y = 0
}
