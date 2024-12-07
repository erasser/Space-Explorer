using UnityEngine;
using UnityEngine.AI;
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
    bool _canSeePlayerAndIsInShootingRange;
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
    float _strafeDirAndSpeed;  // 'dir' is negative or absolute value 

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
        _sqrShootingRange = Mathf.Sqrt(shootingRange);
        _tr = transform;
        _keepingDistanceFromPlayer = shootingRange * Random.Range(.4f, .7f);
        _initialStoppingDistance = _agent.stoppingDistance;
        _weaponTransform = weapon.transform;
    }

    // Některé věci by stačilo checkovat třeba 10× za sekundu
    void Update()
    {
        CalculateValues();

        CheckCanSeePlayer();

        CheckContactWithPlayer();

        ProcessStates();

        ProcessShooting();
    }

    void CalculateValues()
    {
        _toTarget = fpsPlayerTransform.position - _tr.position;
        _weaponToTarget = fpsPlayerTransform.position - weapon.transform.position;
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
            isShooting = _canSeePlayerAndIsInShootingRange;

            ApproachEnemy();

            AimOrResetWeapon();
        }
        else
            AimOrResetWeapon(true);
    }

    void ApproachEnemy()
    {
        //var fightingTmpLocation = (transform.position + 5 * transform.right) - fpsPlayerTransform.position
    }

    void ApproachEnemyOld()  // TODO: Chci použít _strafeDirAndSpeed
    {
        if (_toTarget.magnitude > _keepingDistanceFromPlayer)
            _agent.SetDestination(fpsPlayerTransform.position);  // TODO: Stačí aktualizovat méně často
        else
        {
            var angle = Mathf.Min(Time.deltaTime * _agent.angularSpeed, _horizontalToTargetAngle) * (_horizontalToTargetAngleSigned > 0 ? 1 : -1);

            _tr.Rotate(Vector3.up, angle);
        }
    }

    void StrafeAroundEnemy()  // TODO: To nepoužiju, odstranit, až bude movement hotovej
    {
        transform.Translate(_strafeDirAndSpeed * Time.deltaTime * Vector3.right);
    }

    void SetStateRandomRoaming(bool setNewDestination = false)
    {
        var a = 40;
        _state = State.RandomRoaming;
        _agent.stoppingDistance = _initialStoppingDistance;

        if (!setNewDestination)
            return;

        Vector3 dest = new(Random.Range(-a, a), 1000, Random.Range(-a, a));

        if (Physics.Raycast(dest, Vector3.down, out var hit, Mathf.Infinity, Wc.groundLayer))
            dest = new(dest.x, hit.point.y, dest.z);
        else
            print("no ground hit");
        
        var result = _agent.SetDestination(dest);

        if (!result)
        {
            print("• Destination not set!");
            _agent.SetDestination(Vector3.zero);
        }
    }

    void SetStateFightingPlayer()
    {
        if (_state == State.FightingPlayer)
            return;

        _state = State.FightingPlayer;

        _agent.stoppingDistance = _keepingDistanceFromPlayer;
    }

    void CheckContactWithPlayer()
    {
        if (_canSeePlayerAndIsInShootingRange)
            SetStateFightingPlayer();
    }

    // bool IsPlayerInRange()
    // {
    //     return _toTarget.sqrMagnitude < sqrShootingRange;
    // }

    void CheckCanSeePlayer()
    {
        _canSeePlayerAndIsInShootingRange = _horizontalToTargetAngle < maxHorizontalSightAngle &&
                                            _verticalToTargetAngle < maxVerticalSightAngle;

        //!Physics.Raycast(weapon.barrel.position, _tr.forward, _sqrShootingRange);
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
        var targetVector = reset ? Vector3.zero : _weaponToTarget;
        var resultVector = Vector3.RotateTowards(_weaponTransform.forward, targetVector, Time.deltaTime * weaponOrientationSpeed, 0);
        _weaponTransform.LookAt(_weaponTransform.position + resultVector);
        _weaponTransform.localEulerAngles = new(_weaponTransform.localEulerAngles.x, 0, 0);
    }
}
