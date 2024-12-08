using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using static FPSPlayer;
using static WorldController;

public class FPSEnemy : MonoBehaviour
{
    NavMeshAgent _agent;
    State _state;
    Vector3 _targetLocation;
    Vector3 _toTarget;
    Vector3 _weaponToTarget;
    public float shootingRange = 30;
    float _sqrShootingRange;
    Transform _tr;
    [HideInInspector]
    public bool isShooting;
    bool _canSeePlayer;
    bool _isPlayerInShootingRange;
    public FPSWeapon weapon;
    Transform _weaponTransform;
    float _lastShootTime;
    [Tooltip("Angles which can an enemy see the player within")]
    public float maxHorizontalSightAngle = 8;
    public float maxVerticalSightAngle = 60;
    float _keepingDistanceFromPlayer;
    float _initialStoppingDistance;
    float _horizontalToTargetAngle;
    float _horizontalToTargetAngleSigned;
    float _verticalToTargetAngle;
    float _verticalToTargetAngleSigned;
    public float weaponOrientationSpeed = 5;
    public float horizontalAimingSpeed = 4;

    enum State
    {
        StandingStill,
        RandomRoaming,
        FollowingTarget,
        FightingPlayer
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        SetStateRandomRoaming(true);
        _sqrShootingRange = Mathf.Pow(shootingRange, 2);
        _tr = transform;
        // _keepingDistanceFromPlayer = shootingRange * Random.Range(.4f, .7f);
        _keepingDistanceFromPlayer = 3;
        // _initialStoppingDistance = _agent.stoppingDistance;
        _weaponTransform = weapon.transform;
    }

    // Některé věci by stačilo checkovat třeba 10× za sekundu
    void Update()
    {
        CalculateValues();

        CheckCanSeePlayer();
        CheckIsPlayerInShootingRange();

        CheckContactWithPlayer();

        ProcessStates();

        ProcessShooting();
    }

    void CalculateValues()
    {
        _toTarget = FPSCameraTransform.position - _tr.position;
        _weaponToTarget = FPSCameraTransform.position - weapon.transform.position;
        _horizontalToTargetAngleSigned = Vector2.SignedAngle(new(_toTarget.normalized.x, _toTarget.normalized.z), new(_tr.forward.x, _tr.forward.z));
        _horizontalToTargetAngle = Mathf.Abs(_horizontalToTargetAngleSigned);
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
                SetStateRandomRoaming(true);
                // }
            }
        }

        if (_state == State.FightingPlayer)
        {
            isShooting = _canSeePlayer && _isPlayerInShootingRange;

            FightingManeuver();
        }

        AimOrResetWeapon(_state != State.FightingPlayer);
    }

    void FightingManeuver()
    {
        var resultVector = Vector3.RotateTowards(transform.forward, _toTarget, Time.deltaTime * horizontalAimingSpeed, 0);
        transform.LookAt(transform.position + resultVector);
        transform.localEulerAngles = new(0, transform.localEulerAngles.y, 0);

        if (_agent.remainingDistance <= _agent.stoppingDistance)
            SetFightingDestination(110);

        if (!_isPlayerInShootingRange)
            SetFightingDestination(20);

        void SetFightingDestination(float halfAngle)  // halfAngle = 0 => to player
        {
            var direction = Quaternion.Euler(0, Random.Range(- halfAngle, halfAngle), 0) * new Vector3(_toTarget.x, 0, _toTarget.z).normalized;
            var distance = Random.Range(3f, 15f);

            SetDestination(transform.position + direction * distance);
        }
    }

    void SetStateRandomRoaming(bool setNewDestination = false)
    {
        var a = 50f;
        _state = State.RandomRoaming;
        // _agent.stoppingDistance = _initialStoppingDistance;

        if (!setNewDestination)
            return;

        SetDestination(Random.Range(- a, a), Random.Range(- a, a));
    }

    void SetDestination(float x, float z)
    {
        var result = _agent.SetDestination(GetNavmeshPoint(x, z));

        if (!result)
        {
            print("• Destination not set!");
            _agent.SetDestination(Vector3.zero);
        }
    }

    void SetDestination(Vector3 dest)
    {
        SetDestination(dest.x, dest.z);
    }

    void SetStateFightingPlayer()
    {
        if (_state == State.FightingPlayer)
            return;

        _state = State.FightingPlayer;
    }

    Vector3 GetNavmeshPoint(float x, float z)
    {
        if (Physics.Raycast(new(x, 1000, z), Vector3.down, out var hit, Mathf.Infinity, Wc.groundLayer))
        {
            return new(x, hit.point.y, z);
        }

        print("No ground hit, returning 0, 0, 0");
        return Vector3.zero;
    }

    void CheckContactWithPlayer()
    {
        if (_canSeePlayer)
            SetStateFightingPlayer();
    }

    // bool IsPlayerInRange()
    // {
    //     return _toTarget.sqrMagnitude < sqrShootingRange;
    // }

    void CheckCanSeePlayer()
    {
        _canSeePlayer = _horizontalToTargetAngle < maxHorizontalSightAngle &&
                                            _verticalToTargetAngle < maxVerticalSightAngle;
        //!Physics.Raycast(weapon.barrel.position, _tr.forward, _sqrShootingRange);
    }

    void CheckIsPlayerInShootingRange()
    {
        _isPlayerInShootingRange = _toTarget.sqrMagnitude < _sqrShootingRange;
    }

    void ProcessShooting()
    {
        if (!isShooting)
            return;

        if (Time.time > _lastShootTime + weapon.projectilePrefab.delay)
        {
            weapon.Shoot();
            _lastShootTime = Time.time;
        }
    }

    void AimOrResetWeapon(bool reset = false)
    {
        if (reset && _weaponTransform.localEulerAngles == Vector3.zero)
            return;

        var targetVector = reset ? Vector3.zero : _weaponToTarget;
        var resultVector = Vector3.RotateTowards(_weaponTransform.forward, targetVector, Time.deltaTime * weaponOrientationSpeed, 0);
        _weaponTransform.LookAt(_weaponTransform.position + resultVector);
        _weaponTransform.localEulerAngles = new(_weaponTransform.localEulerAngles.x, 0, 0);
    }
}
