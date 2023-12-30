using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Ship;
using static UniverseController;

// TODO: Patrol
// TODO: Zajistit, aby byla cesta korektní po kolizi (tj. změny pozice) objektu.
// ↑ Funguje to, ale asi jen do chvíle, než objekt dotrkám k dalšímu checkpoinu a on se bude snažit vrátit k tomu aktuálnímu.
// ↑ Asi by to v případě kolize chtělo najít nejbližší checkpoint. TODO 

[RequireComponent(typeof(Rigidbody), typeof(LineRenderer))]
public class MyNavMeshAgent : CachedMonoBehaviour
{
    public bool showPath = true;
    public float targetMinDistance = 2f;
    public float agentFixedUpdateDeltaTime = .2f;
    float _lastAgentFixedUpdate;
    Ship _ship;
    NavMeshPath _navMeshPath;
    readonly List<Vector3> _pathPoints = new();
    int _actualPathPointIndex = -1;
    float _targetSqrMinDistance;
    LineRenderer _lineRenderer;
    GameObject _pointVisualizer;
    // Vector3 _actualPathPoint;
    Vector3 _toActualPathPointDirection;
    State _state;
    AiPilot _aiPilot;
    bool _needsRegeneratePath;
    // TARGETS and DESTINATIONS  (helper class to be created)
    Ship _target;
    Vector3 _destination;
    // readonly List<Vector3> _destinations = new(new Vector3[2]);  // TODO: Just two for now
    // int _remainingPathPoints;
    // int _actualDestinationIndex;

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
        FollowingEnemy,
        RandomRoaming
    }

    void Start()
    {
        _ship = GetComponent<Ship>();
        _lineRenderer = GetComponent<LineRenderer>();
        _navMeshPath = new();
        _lastAgentFixedUpdate = Time.time;
        _targetSqrMinDistance = Mathf.Pow(targetMinDistance, 2);
        _aiPilot = GetComponent<AiPilot>();
    }

    void FixedUpdate()
    {
        if (Time.time > _lastAgentFixedUpdate + agentFixedUpdateDeltaTime)
        {
            AgentFixedUpdate();
            _lastAgentFixedUpdate = Time.time;
        }

        // InfoText.text = "state: " + _state;
        // InfoText.text += "\npath points: index " + _actualPathPointIndex + " / " + _pathPoints.Count + " count";
        // _toActualPathPointDirection = _actualPathPoint - transformCached.position;
        // _toActualPathPointDirection = _pathPoints[_actualPathPointIndex] - transformCached.position;

        if (_needsRegeneratePath)
        {
            ReGeneratePath();

            _needsRegeneratePath = false;
        }

        if (_state == State.GoingToDestination)
        {
            // // CheckPathPointReached();
            //
            // if (_state == State.StandingStill)
            //     return;

            // _toActualPathPointDirection = _actualPathPoint - transformCached.position;
            _toActualPathPointDirection = GetCurrentPathPoint() - transformCached.position;
            _ship.SetMoveVectorHorizontal(_toActualPathPointDirection.x);
            _ship.SetMoveVectorVertical(_toActualPathPointDirection.z);
        }
    }

    Vector3 GetCurrentPathPoint()
    {
        return _pathPoints[_actualPathPointIndex];
    }

    void AgentFixedUpdate()
    {
        if (_state == State.StandingStill)
            return;

        if (_state == State.GoingToDestination)
        {
            CheckPathPoints();

            // PredictCollisions();
        }

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
        
        /*if (_state == State.GoingToDestination)
        {
            if (!GeneratePathTo(_destination))
            {
                Debug.LogWarning("◘ NO VALID PATH FOUND! "); // TODO: Najednou se nejde dostat do cíle
                return;
            }
        }*/

    }

    bool IsPathPointReached()
    {
        // InfoText.text = _toActualPathPointDirection.sqrMagnitude.ToString();
        // InfoText.text = _toActualPathPointDirection.sqrMagnitude + "\n" + _targetSqrMinDistance; 
        return _toActualPathPointDirection.sqrMagnitude <= _targetSqrMinDistance;
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
            print("☺ Key is beyond the pathPoints count, returning");
            return;
        }

        if (_actualPathPointIndex == -1)
        {
            print("☻ Key is -1");
            return;
        }
// print("count: " + _pathPoints.Count + ", index: " + _actualDestinationIndex);
        // _toActualPathPointDirection = _pathPoints[_actualPathPointIndex] - transformCached.position;
        var distance = _actualPathPointIndex == _pathPoints.Count - 1 ? _targetSqrMinDistance * 1.5f : _targetSqrMinDistance;  // Break sooner before last path point
        var reached = _toActualPathPointDirection.sqrMagnitude <= distance;
        // InfoText.text = Mathf.Round(_toActualPathPointDirection.sqrMagnitude) + " <\n" + distance + " ?";
        // print(_toActualPathPointDirection.sqrMagnitude + ", " + distance);

        if (reached && ++_actualPathPointIndex == _pathPoints.Count)
        {
            // print("• reached!");
            // Stop();
            _aiPilot.GoToRandomLocation();
        }
    }

    /*bool CheckPathPointReached()
    {
        // _toActualPathPointDirection = _actualPathPoint - transformCached.position;
        // InfoText.text = (Mathf.Round(_toActualPathPointDirection.magnitude * 10)/10).ToString();
        
        // var distance = _actualPathPointIndex == _pathPoints.Count - 1 ? _targetSqrMinDistance * 1.5f : _targetSqrMinDistance;  // Break sooner before last path point
        var reached = _toActualPathPointDirection.sqrMagnitude <= 16;  // TODO...

        if (reached)
        {
            // print("checkpoint reached!");

            if (_remainingPathPoints == 0)
            {
                // print("That was last checkpoint!");

                // Stop();
                _aiPilot.GoToRandomLocation();

            }
            // else
            //     GeneratePathTo();
            return true;
        }

        return false;
    }*/
    
    bool GeneratePathTo(Vector3 targetLocation)
    {
        // CheckPathPoint();
        //
        // if (_actualPathPointIndex == -1)
        // {
        //     print("• _actualPathPointIndex = -1");
        //     return false;
        // }

        var position = transformCached.position;
        var result = NavMesh.CalculatePath(new(position.x, 0, position.z), targetLocation, NavMesh.AllAreas, _navMeshPath);

        if (!result)
            return false;

        // _actualPathPointIndex = 0;
        _pathPoints.Clear();
        _pathPoints.AddRange(_navMeshPath.corners);
        _actualPathPointIndex = 0;

        // _pathPoints = new(_navMeshPath.corners);

        // var count = _navMeshPath.corners.Length;        
        // for (var i = 1; i < count; ++i)
        // _pathPoints.Add(_navMeshPath.corners[i]);

        // _actualPathPoint = _navMeshPath.corners[1];
        // _remainingPathPoints = _navMeshPath.corners.Length - 2;

        ShowPath();

        return true;
    }

    public bool SetDestination(Vector3 target /*, TravelStance travelStance = TravelStance.PreferDestination*/)
    {
        if (!GeneratePathTo(target))
            return false;

        // _destinations[0] = target;
        // _actualPathPoint = target;
        _destination = target;
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
        print("STOP!");
        _pathPoints.Clear();
        _actualPathPointIndex = -1;
        _state = State.StandingStill;

        _ship.SetMoveVectorHorizontal(0);
        _ship.SetMoveVectorVertical(0);
        // GetComponent<Rigidbody>().velocity = Vector3.zero;   ??? ??? ???
    }

    bool IsEnemyInRange()
    {
        var sqrDistance = (ActiveShip.transformCached.position - transformCached.position).sqrMagnitude;
        return sqrDistance <= ShootingSqrRange;
    }

    public void Patrol(Vector3 destinationPoint)
    {
        // TODO: Checknout situaci, kdy 2. bod je už nadosah

        // _destinations[0] = transformCached.position;
        // _destinations[1] = destinationPoint;
        
        // ...        

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

    void OnCollisionExit(Collision other)
    {
        if (_state != State.StandingStill)
        {
            print("collision: " + other.collider.name);
            // ReGeneratePath();
            _needsRegeneratePath = true;
        }
    }

    bool ReGeneratePath()
    {
        print("► REGENERATING PATH ◄");
        return GeneratePathTo(GetCurrentDestination());
    }

    Vector3 GetCurrentDestination()
    {
        if (_state == State.GoingToDestination)
            return _destination;

        Debug.LogWarning("TODO !");
        if (_state == State.FollowingEnemy)
            return _pathPoints[_actualPathPointIndex];  // TODO
        if (_state == State.Patrolling)
            return _pathPoints[_actualPathPointIndex];  // TODO
        if (_state == State.RandomRoaming)
            return _pathPoints[_actualPathPointIndex];  // TODO

        return transform.position;
    }

    void PredictCollisions()
    {
        foreach (Ship ship in ShipList)
        {
            // Debug.DrawRay(transformCached.position, SetVectorLength(ship.transformCached.position - transformCached.position, 30), Color.cyan);
            var result = Physics.Raycast(transformCached.position, ship.transformCached.position - transformCached.position, out RaycastHit hit, 30, LayerMask.NameToLayer("predictive collider"));

            if (result)
            {
                print(name + ": Predicted collision with: " + hit.collider.name);
                Stop();
            }
        }
    }

    public void PauseMotion()
    {
        
    }

    public void ResumeMotion()
    {
        
    }
    
    
    // TODO: Nějak low-level zajistit, aby cíl měl vždy y = 0
}
