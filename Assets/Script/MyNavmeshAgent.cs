using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UniverseController;
// TODO: Zajistit, aby byla cesta korektní po kolizi (tj. změny pozice) objektu

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(LineRenderer))]
public class MyNavMeshAgent : CachedMonoBehaviour
{
    public bool showPath = true;
    public float targetMinDistance = 2f;
    public float agentFixedUpdateDeltaTime = .2f;
    float _lastAgentFixedUpdate;
    Ship _ship;
    // Rigidbody _rb;
    NavMeshPath _navMeshPath;
    List<Vector3> _pathPoints = new();
    int _actualPathPointIndex;
    float _targetSqrMinDistance;
    LineRenderer _lineRenderer;
    GameObject _pointVisualizer;
    // Vector3 _target;
    State _state;
    Vector3 _toActualPathPointDirection;

    enum State
    {
        StandingStill,
        Going
    }

    void Start()
    {
        _ship = GetComponent<Ship>();
        // _rb = GetComponent<Rigidbody>();
        _lineRenderer = GetComponent<LineRenderer>();
        _navMeshPath = new();
        _lastAgentFixedUpdate = Time.time;
        _targetSqrMinDistance = Mathf.Pow(targetMinDistance, 2);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            GeneratePathTo(new (-180, 0, 0));

        // if (Time.time > _lastAgentFixedUpdate + agentFixedUpdateDeltaTime)
        //     AgentFixedUpdate();

        
        if (_state == State.Going)
        {
            // transform.LookAt(_pathPoints[_actualPathPointIndex]);
            _toActualPathPointDirection = _pathPoints[_actualPathPointIndex] - transformCached.position;
            _ship.SetMoveVectorHorizontal(_toActualPathPointDirection.x);
            _ship.SetMoveVectorVertical(_toActualPathPointDirection.z);
        }
        
        if (_state == State.Going)
        {
            if (IsPathPointReached())
            {
                ++_actualPathPointIndex;

                if (_actualPathPointIndex == _pathPoints.Count)  // last point
                    Stop();
            }
        }
    }

    void AgentFixedUpdate()
    {
        _lastAgentFixedUpdate = Time.time;

        // if (_state == State.Going)
        // {
        //     if (IsPathPointReached())
        //     {
        //         ++_actualPathPointIndex;
        //
        //         if (_actualPathPointIndex == _pathPoints.Count)  // last point
        //             Stop();
        //     }
        // }
    }

    bool IsPathPointReached()
    {
        return _toActualPathPointDirection.sqrMagnitude <= _targetSqrMinDistance;
    }

    void GeneratePathTo(Vector3 targetLocation)
    {
        NavMesh.CalculatePath(transformCached.position, targetLocation, NavMesh.AllAreas, _navMeshPath);
        _actualPathPointIndex = 0;
        _pathPoints.Clear();

        var count = _navMeshPath.corners.Length;        
        for (var i = 1; i < count; ++i)
            _pathPoints.Add(_navMeshPath.corners[i]);

        _state = State.Going;

        ShowPath();
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

    // public void SetTargetV3(Vector3 target)
    // {
    //     _target = target;
    // }

    public void GoTo(Vector3 target)
    {
        GeneratePathTo(target);

        _state = State.Going;
    }

    public void Stop()
    {
        _state = State.StandingStill;
        _ship.SetMoveVectorHorizontal(0);
        _ship.SetMoveVectorVertical(0);
    }

    public void PauseMotion()
    {
        
    }

    public void ResumeMotion()
    {
        
    }
}
