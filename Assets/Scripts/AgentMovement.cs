using System.Collections.Generic;
using UnityEngine;
using IA.Steering;

public class AgentMovement : MonoBehaviour
{
    [SerializeField]
    private bool _enabled = true;
    [SerializeField]
    private bool _playerControlled = false;
    private bool _kinematicMode = false;
    private bool _followingPath = false;

    [SerializeField]
    private AgentMovementVars _agentMoveVars;

    private ISteering? _baseSteering = null;
    private ISteering? _wallAvoidanceSteering = null;
    private ISteering? _lookSteering = null;
    private IA.Steering.Dynamic.AgentsAvoidance _agentAvoidanceSteering = null;
    private IA.Steering.Dynamic.Separation _separationSteering = null;

    [Header("Wall Avoidance")]
    [SerializeField]
    private List<AvoidanceRay> _avoidanceRays = new List<AvoidanceRay>();
    [SerializeField]
    private Transform _avoidanceRaysOrigin = null;

    [Header("Agent Avoidance")]
    [SerializeField]
    private int _framesBtwAAvoid = 5;
    private int _curAAvoidFrame = 0;
    private Vector2 _lastAAvoidForce = Vector2.zero;

    [Header("Separation")]
    [SerializeField]
    private float _minSeparation = 1f;
    [SerializeField]
    private int _framesBtwSep = 5;
    private int _curSepFrame = 0;
    private Vector2 _lastSepForce = Vector2.zero;

    [Header("Layers")]
    [SerializeField]
    private LayerMask _agentsMask = 0;
    [SerializeField]
    private LayerMask _wallMask = 0;

    [Header("Testing")]
    public AgentMovement testTarget;

    public bool IsEnabled
    {
        get { return _enabled; }
    }
    public bool IsKinematic
    {
        get { return _kinematicMode; }
    }

    public ref readonly AgentMovementVars MoveVars
    {
        get { return ref _agentMoveVars; }
    }

    public KinematicMovementData KinematicData { get; private set; }


    private void Awake()
    {
        MatchTransform();

        if (_avoidanceRays.Count > 0 && _avoidanceRaysOrigin is null)
            throw new System.NullReferenceException(nameof(_avoidanceRaysOrigin));

        _curSepFrame = Random.Range(0, 5);
        _curAAvoidFrame = Random.Range(0, 5);
    }

    private void Update()
    {
        MatchTransform();

        if (!_enabled)
            return;

        if (_playerControlled)
            ControlledUpdate();
        else if (_kinematicMode)
            KinematicUpdate();
        else
            DynamicUpdate();

#if UNITY_EDITOR
        if (Application.isPlaying)
            Debug.DrawRay(KinematicData.Position, KinematicData.Velocity, Color.red);
#endif
    }

    private void LateUpdate()
    {
        if (_avoidanceRays.Count == 0)
            return;

        float z = _avoidanceRaysOrigin.transform.eulerAngles.z;
        float newZ = MathUtils.NewOrientation(_avoidanceRaysOrigin.transform.eulerAngles.z, KinematicData.Velocity);
        if (newZ != z)
            newZ *= Mathf.Rad2Deg;
        _avoidanceRaysOrigin.transform.eulerAngles = new Vector3(0f, 0f, newZ)
     ;
    }


    private void KinematicUpdate()
    {
        SteeringOutput baseSteer = _baseSteering?.GetSteering() ?? SteeringOutput.NoSteering;
        Debug.DrawRay(KinematicData.Position, baseSteer.linear, Color.green);

        KinematicData.Position += KinematicData.Velocity * Time.deltaTime;
        KinematicData.Orientation += KinematicData.Rotation * Time.deltaTime;

        (float orRads, float orDegs) = MathUtils.WrapOrientation(KinematicData.Orientation);
        KinematicData.Orientation = orRads;

        KinematicData.Velocity = baseSteer.linear;
        KinematicData.Rotation = baseSteer.angular;

        if (KinematicData.Velocity.magnitude > _agentMoveVars.MaxSpeed)
            KinematicData.Velocity = KinematicData.Velocity.normalized * _agentMoveVars.MaxSpeed;

        // Update transform here!
        transform.position = KinematicData.Position;
        transform.eulerAngles = new Vector3(0f, 0f, orDegs);
    }


    private void DynamicUpdate()
    {
        SteeringOutput baseSteer = _baseSteering?.GetSteering() ?? SteeringOutput.NoSteering;
        SteeringOutput avoidSteer = _wallAvoidanceSteering?.GetSteering() ?? SteeringOutput.NoSteering;
        SteeringOutput separationSteer = HandleSeparation();
        SteeringOutput agentAvoidSteer = HandleAgentAvoidance();
        SteeringOutput lookSteer = _lookSteering?.GetSteering() ?? SteeringOutput.NoSteering;

        if (ArrivedPathGoal())
            StopFollowPath();

#if UNITY_EDITOR
        Debug.DrawRay(KinematicData.Position, baseSteer.linear, Color.green);
        Debug.DrawRay(KinematicData.Position, avoidSteer.linear, Color.blue);
        Debug.DrawRay(KinematicData.Position, separationSteer.linear, Color.black);
        Debug.DrawRay(KinematicData.Position, agentAvoidSteer.linear, Color.magenta);
        Debug.DrawRay(KinematicData.Position, baseSteer.linear + avoidSteer.linear + separationSteer.linear + agentAvoidSteer.linear, Color.cyan);
#endif
        KinematicData.Position += KinematicData.Velocity * Time.deltaTime;
        KinematicData.Orientation += KinematicData.Rotation * Time.deltaTime;

        (Vector2 linFric, float angFric) = CalculateFriction();

        KinematicData.Velocity -= linFric * Time.deltaTime;
        KinematicData.Rotation -= angFric * Time.deltaTime;

        if (KinematicData.Velocity.magnitude <= MathUtils.STOP_EPSILON)
            KinematicData.Velocity = Vector2.zero;

        (float orRads, float orDegs) = MathUtils.WrapOrientation(KinematicData.Orientation);
        KinematicData.Orientation = orRads;

        KinematicData.Velocity += (baseSteer.linear + 2f * avoidSteer.linear + 7f * separationSteer.linear + 2.5f * agentAvoidSteer.linear) * Time.deltaTime;
        KinematicData.Rotation += (baseSteer.angular + lookSteer.angular) * Time.deltaTime;

        if (KinematicData.Velocity.magnitude <= MathUtils.STOP_EPSILON)
            KinematicData.Velocity = Vector2.zero;

        if (KinematicData.Velocity.magnitude > _agentMoveVars.MaxSpeed)
            KinematicData.Velocity = KinematicData.Velocity.normalized * _agentMoveVars.MaxSpeed;

        // Update transform here!
        transform.position = KinematicData.Position;
        transform.eulerAngles = new Vector3(0f, 0f, orDegs);
    }


    private void ControlledUpdate()
    {
        SteeringOutput steering = InputToSteering();

        KinematicData.Position += KinematicData.Velocity * Time.deltaTime;
        KinematicData.Orientation += KinematicData.Rotation * Time.deltaTime;

        (float orRads, float orDegs) = MathUtils.WrapOrientation(KinematicData.Orientation);
        KinematicData.Orientation = orRads;

        KinematicData.Velocity = steering.linear;
        KinematicData.Rotation = steering.angular;

        if (KinematicData.Velocity.magnitude > _agentMoveVars.MaxSpeed)
            KinematicData.Velocity = KinematicData.Velocity.normalized * _agentMoveVars.MaxSpeed;

        // Update transform here!
        transform.position = KinematicData.Position;
        transform.eulerAngles = new Vector3(0f, 0f, orDegs);
    }


    private SteeringOutput HandleAgentAvoidance()
    {
        _curAAvoidFrame = (_curAAvoidFrame + 1) % _framesBtwAAvoid;
        if (_agentAvoidanceSteering is null)
            return SteeringOutput.NoSteering;

        if (_curAAvoidFrame != 0)
            return new SteeringOutput(_lastAAvoidForce, 0f);

        List<KinematicMovementData> targets = new();
        Collider2D[] hits = Physics2D.OverlapCircleAll(KinematicData.Position, _agentMoveVars.Radius * 2f, _agentsMask);
        foreach (Collider2D h in hits)
        {
            if (!h.TryGetComponent(out AgentMovement agent) || agent == this)
                continue;

            if (agent == this)
                continue;

            targets.Add(agent.KinematicData);
        }

        _agentAvoidanceSteering.Targets = targets;
        SteeringOutput final = _agentAvoidanceSteering.GetSteering();
        _lastAAvoidForce = final.linear;
        return final;
    }

    private SteeringOutput HandleSeparation()
    {
        _curSepFrame = (_curSepFrame + 1) % _framesBtwSep;
        if (_separationSteering is null)
            return SteeringOutput.NoSteering;

        if (_curSepFrame != 0)
            return new SteeringOutput(_lastSepForce, 0f);

        List<StaticMovementData> targets = new();
        Collider2D[] hits = Physics2D.OverlapCircleAll(KinematicData.Position, _minSeparation, _agentsMask);
        foreach (Collider2D h in hits)
        {
            if (!h.TryGetComponent(out AgentMovement agent) || agent == this)
                continue;

            if (agent == this)
                continue;

            targets.Add(agent.KinematicData);
        }

        _separationSteering.Targets = targets;
        SteeringOutput final = _separationSteering.GetSteering();
        _lastSepForce = final.linear;
        return final;
    }


    private (Vector2 linFric, float angFric) CalculateFriction() =>
        (
        KinematicData.Velocity.normalized * _agentMoveVars.LinearFriction, 
        KinematicData.Rotation * _agentMoveVars.AngularFriction
        );


    private SteeringOutput InputToSteering()
    {
        Vector2 vel = new(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float ang = Input.GetAxisRaw("HorizontalR");
        return new SteeringOutput(vel.normalized * _agentMoveVars.MaxSpeed, _agentMoveVars.MaxAngularRad * ang);
    }

    private bool ArrivedPathGoal() =>
        _followingPath && (_baseSteering as IA.Steering.Dynamic.FollowPath).Arrived;

    private void MatchTransform()
    {
        if (KinematicData is null)
        {
            KinematicData = new KinematicMovementData(transform.position, transform.eulerAngles.z * Mathf.Deg2Rad, Vector2.zero, 0f);
            return;
        }

        KinematicData.Position = transform.position;
        KinematicData.Orientation = transform.eulerAngles.z * Mathf.Deg2Rad;
    }

    public void EnableKinematicSeek()
    {
        _kinematicMode = true;
        _baseSteering = new IA.Steering.Kinematic.Seek(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableKinematicArrive()
    {
        _kinematicMode = true;
        _baseSteering = new IA.Steering.Kinematic.Arrive(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableKinematicFlee()
    {
        _kinematicMode = true;
        _baseSteering = new IA.Steering.Kinematic.Flee(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableKinematicWander()
    {
        _kinematicMode = true;
        _baseSteering = new IA.Steering.Kinematic.Wander(KinematicData, _agentMoveVars);
    }

    public void EnableDynamicSeek()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.Seek(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableDynamicArrive()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.Arrive(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableDynamicFlee()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.Flee(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableDynamicVelocityMatching()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.VelocityMatch(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableDynamicAlign()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.Align(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableDynamicPursue()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.Pursue(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableDynamicEvade()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.Evade(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableDynamicFace()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.Face(KinematicData, testTarget.KinematicData, _agentMoveVars);
    }

    public void EnableDynamicWander()
    {
        _kinematicMode = false;
        _baseSteering = new IA.Steering.Dynamic.Wander(KinematicData, _agentMoveVars, 2f, 0.5f, 0.4f, Random.Range(0, 360f) * Mathf.Deg2Rad);
    }

    public void ToggleWallAvoidance()
    {
        _kinematicMode = false;
        if (_wallAvoidanceSteering != null)
        {
            _wallAvoidanceSteering = null;
            return;
        }

        _wallAvoidanceSteering = new IA.Steering.Dynamic.ObstacleAvoidance(KinematicData, _avoidanceRays, _wallMask, _agentMoveVars);
    }

    public void ToggleAgentAvoidance()
    {
        _kinematicMode = false;
        if (_agentAvoidanceSteering != null)
        {
            _agentAvoidanceSteering = null;
            return;
        }

        _agentAvoidanceSteering = new IA.Steering.Dynamic.AgentsAvoidance(KinematicData, _agentMoveVars);
    }

    public void ToggleSeparation()
    {
        _kinematicMode = false;
        if (_separationSteering != null)
        {
            _separationSteering = null;
            return;
        }

        _separationSteering = new IA.Steering.Dynamic.Separation(KinematicData, _agentMoveVars, _minSeparation);
    }

    public void ToggleLookVelocity()
    {
        _kinematicMode = false;
        if (_lookSteering != null)
        {
            _lookSteering = null;
            return;
        }

        _lookSteering = new IA.Steering.Dynamic.LookVelocity(KinematicData, _agentMoveVars);
    }

    public void StartFollowPath(List<Vector2> path, bool loop)
    {
        _kinematicMode = false;
        if (path is null || path.Count == 0)
        {
            Debug.LogWarning("[AI.STEERING] Trying to follow an empty path");
            return;
        }

        if (path.Count == 1 && loop)
        {
            Debug.LogWarning("[AI.STEERING] Trying to loop a single node path");
            return;
        }

        _baseSteering = new IA.Steering.Dynamic.FollowPath(KinematicData, _agentMoveVars, path, loop);
    }


    public void StopFollowPath()
    {
        if (!_followingPath)
        {
            Debug.LogWarning("[AI.STEERING] Trying to stop following a path, but the agent is not following one.");
            return;
        }

        _baseSteering = null;
        _followingPath = false;
    }

    public void ClearBaseMovement()
    {
        _baseSteering = null;
        _followingPath = false;
    }


    private void OnDrawGizmos()
    {
        if (_wallAvoidanceSteering is null)
            return;

        Gizmos.color = Color.magenta;
        foreach (AvoidanceRay r in _avoidanceRays)
            Gizmos.DrawRay(r.Position, r.Ray);
    }
}
