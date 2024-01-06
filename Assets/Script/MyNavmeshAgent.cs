using System.Collections.Generic;
using UnityEditor;
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
    const float CollisionPredictionMaxDistance = 50;
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
    static BoxCollider _predictiveCollider;
    static Transform _predictiveColliderTransform;
    static LayerMask _predictiveColliderLayerMask;
    static readonly List<MyNavMeshAgent> MyNavMeshAgents = new();  // for collision predicting
    Vector3 _predictiveBoxCastSize;

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
        MyNavMeshAgents.Add(this);
        _predictiveColliderLayerMask = LayerMask.NameToLayer("predictive collider");
        SetBounds();
    }

    void FixedUpdate()
    {
        ProcessAgentFixedUpdateDeltaTime();

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

    void ProcessAgentFixedUpdateDeltaTime()
    {
        if (Time.time > _lastAgentFixedUpdate + agentFixedUpdateDeltaTime)
        {
            AgentFixedUpdate();
            _lastAgentFixedUpdate = Time.time;
        }
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

    Vector3 GetCurrentPathPoint()
    {
        return _pathPoints[_actualPathPointIndex];
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

    /*void PredictCollisions()
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
    }*/

    public static void CreatePredictiveCollider()
    {
        if (_predictiveCollider)
            return;

        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // cube.transform.parent = transform;
        // cube.transform.position = Vector3.zero;  // needed?
        cube.layer = LayerMask.NameToLayer("predictive collider");
        _predictiveCollider = cube.GetComponent<BoxCollider>();
        _predictiveCollider.isTrigger = true;
        _predictiveCollider.includeLayers = LayerMask.NameToLayer("predictive collider");
        _predictiveCollider.excludeLayers = Physics.AllLayers;
        _predictiveColliderTransform = _predictiveCollider.transform;
        // Destroy(cube.GetComponent<Renderer>());
        cube.SetActive(false);
    }

    void UpdatePredictiveColliderFor()
    {
        
    }

    public static void PredictCollisions()
    {
        int count = MyNavMeshAgents.Count;

        if (count < 2)
            return;

        _predictiveCollider.gameObject.SetActive(true);

        for (int first = 0; first < count - 1; ++first)
        {
            for (int second = first + 1; second < count; ++second)
            {
                // TODO: Filtrovat zde. Pouze když je alespoň jeden jedoucí, pouze do určité vzdálenosti.
                PredictCollision(MyNavMeshAgents[first], MyNavMeshAgents[second]);
            }
        }

        _predictiveCollider.gameObject.SetActive(true);
    }

    static void PredictCollision(MyNavMeshAgent obj1, MyNavMeshAgent obj2)
    {
        // TODO: Každej box je jinak velkej, přestože vycházejí ze stejných modelů
        // TODO: Overlapování bez výsledku
        // TODO: Prodlužovat ty boxy jen vodorovně (například pro nakloněné lodě čumákem dolů).
        
        // prepare collider to cast against
        var predictionColliderPosition = GetPredictedPositionOffset(obj2._ship, obj1._ship, obj1._ship.rb.velocity.magnitude);
        _predictiveColliderTransform.position = obj2.transformCached.position + predictionColliderPosition;
        _predictiveColliderTransform.rotation = Quaternion.Euler(0, obj2.transformCached.rotation.y, 0);
        _predictiveCollider.size = obj2._predictiveBoxCastSize;

        // box cast to be set
        var boxCastCenterOffset = GetPredictedPositionOffset(obj1._ship, obj2._ship, obj2._ship.rb.velocity.magnitude);

        // var wasHit = Physics.BoxCast(obj1.transformCached.position + boxCastCenterOffset, obj1._predictiveBoxCastSize, obj1._ship.rb.velocity, obj1.transformCached.rotation, Mathf.Infinity, _predictiveColliderLayerMask);

        var rot = Quaternion.Euler(0, obj1.transformCached.rotation.y, 0);
        Collider[] colliders = Physics.OverlapBox(obj1.transformCached.position + boxCastCenterOffset, obj1._predictiveBoxCastSize, rot, _predictiveColliderLayerMask);

        
        if (colliders.Length > 0)
        {
            // print("• predicted collision between " + obj1.name + " and " + obj2.name);
            // EditorApplication.isPaused = true;
            InfoText.text = "• predicted collision between " + obj1.name + " and " + obj2.name;
        }
        else
            InfoText.text = "";

        ExtDebug.DrawBox(_predictiveColliderTransform.position, _predictiveCollider.bounds.extents, _predictiveColliderTransform.rotation, Color.magenta);

        ExtDebug.DrawBox(obj1.transformCached.position + boxCastCenterOffset, obj1._predictiveBoxCastSize, rot, Color.cyan);
        
        Debug.DrawLine(_predictiveColliderTransform.position, obj1.transformCached.position + boxCastCenterOffset, Color.yellow);
    }

    void SetBounds()
    {
        var originalRotation = transformCached.rotation;
        transformCached.rotation = Quaternion.identity;

        foreach (var component in transform.GetComponentsInChildren(typeof(Renderer)))
        {
            var rendererComponent = (Renderer)component;
            if (_predictiveBoxCastSize.x < rendererComponent.bounds.size.x)
                _predictiveBoxCastSize.x = rendererComponent.bounds.size.x;
            if (_predictiveBoxCastSize.y < rendererComponent.bounds.size.y)
                _predictiveBoxCastSize.y = rendererComponent.bounds.size.y;
            if (_predictiveBoxCastSize.z < rendererComponent.bounds.size.z)
                _predictiveBoxCastSize.z = rendererComponent.bounds.size.z;
        }

        transformCached.rotation = originalRotation;
    }

    public void PauseMotion()
    {

    }

    public void ResumeMotion()
    {

    }


    // TODO: Nějak low-level zajistit, aby cíl měl vždy y = 0
}
