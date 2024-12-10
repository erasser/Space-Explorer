using UnityEngine;
using UnityEngine.AI;
using static FPSPlayer;
using static WorldController;
using Random = UnityEngine.Random;

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
    float _horizontalToTargetAngle;
    float _horizontalToTargetAngleSigned;
    float _verticalToTargetAngle;
    float _verticalToTargetAngleSigned;
    public float verticalAimingSpeed = 1;
    static Rigidbody _playerRb;
    public bool predictiveShooting;
    float _projectileSpeed;
    float _debugLastYRotation;
    float _initialAngularSpeed;  // TODO: Zde jsem skončil

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
        _weaponTransform = weapon.transform;
        GetProjectileSpeed();
        _playerRb = fpsPlayer.GetComponent<FirstPersonController>().rb;
    }

    void GetProjectileSpeed()
    {
        var projectile = Instantiate(_weaponTransform.GetComponent<FPSWeapon>().projectilePrefab);
        _projectileSpeed = projectile.GetComponent<FPSProjectile>().speed;
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

        InfoText.text = Mathf.Abs(transform.eulerAngles.y - _debugLastYRotation) + "\n" + _agent.angularSpeed * Time.deltaTime;
        _debugLastYRotation = transform.eulerAngles.y;
    }

    void CalculateValues()
    {
        var targetPosition = GetTargetPosition();

        _toTarget = targetPosition - _tr.position;
        _weaponToTarget = targetPosition - weapon.transform.position;
        _horizontalToTargetAngleSigned = Vector2.SignedAngle(new(_toTarget.normalized.x, _toTarget.normalized.z), new(_tr.forward.x, _tr.forward.z));
        _horizontalToTargetAngle = Mathf.Abs(_horizontalToTargetAngleSigned);
    }

    Vector3 GetTargetPosition()
    {
        return predictiveShooting ? fpsPlayer.targetTransform.position + GetPredictedPositionOffset() : fpsPlayer.targetTransform.position;
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
        var resultVector = Vector3.RotateTowards(transform.forward, _toTarget, _agent.angularSpeed * Time.deltaTime, 0);
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

    void SetStateFightingPlayer()
    {
        if (_state == State.FightingPlayer)
            return;

        _state = State.FightingPlayer;
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
        var resultVector = Vector3.RotateTowards(_weaponTransform.forward, targetVector, Time.deltaTime * verticalAimingSpeed, 0);
        _weaponTransform.LookAt(_weaponTransform.position + resultVector);
        _weaponTransform.localEulerAngles = new(_weaponTransform.localEulerAngles.x, 0, 0);
    }

    Vector3 GetPredictedPositionOffset()
    {
        if (_playerRb.velocity == Vector3.zero || _projectileSpeed == 0)
            return Vector3.zero;

        float a = Vector3.Dot(_playerRb.velocity, _playerRb.velocity) - Mathf.Pow(_projectileSpeed, 2);
        float b = 2 * Vector3.Dot(_playerRb.velocity, _toTarget);
        float c = Vector3.Dot(_toTarget, _toTarget);
        float p = - b / (2 * a);
        float q = Mathf.Sqrt(Mathf.Abs(b * b - 4 * a * c)) / (2 * a);
        float t1 = p - q;
        float t2 = p + q;
        float t;

        if (t1 > t2 && t2 > 0)
            t = t2;
        else
            t = t1;

        return _playerRb.velocity * Mathf.Abs(t);
    }

}
