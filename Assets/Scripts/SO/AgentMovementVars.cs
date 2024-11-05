using UnityEngine;

[CreateAssetMenu(fileName = "MVars_", menuName = "Agent Movement Vars")]
public class AgentMovementVars : ScriptableObject
{
    [SerializeField]
    private float _radius = 2f;
    [SerializeField]
    private float _maxSpeed = 10f;
    [SerializeField]
    private float _maxRotation = 32f;
    [SerializeField]
    private float _maxAcceleration = 3f;
    [SerializeField]
    private float _maxAngular = 15f;
    [SerializeField]
    private float _fleeTreshold = 10f;
    [SerializeField]
    private float _arriveTime = 0.25f;
    [SerializeField]
    private float _prediction = 0.5f;
    [SerializeField]
    private float _linearSlowRadius = 4f;
    [SerializeField]
    private float _linearTargetRadius = 1f;
    [SerializeField]
    private float _angularSlowRadius = 6f;
    [SerializeField]
    private float _angularTargetRadius = 2f;
    [SerializeField]
    private float _avoidanceDistance = 1.5f;
    [SerializeField]
    private float _linearFriction = 2.2f;
    [SerializeField]
    private float _angularFriction = 0.90625f;

    public float Radius { get { return _radius; } }
    public float MaxSpeed { get { return _maxSpeed; } }
    public float MaxAcceleration { get { return _maxAcceleration; } }
    public float MaxRotationRad { get { return _maxRotation * Mathf.Deg2Rad; } }
    public float MaxRotationDeg { get { return _maxRotation; } }
    public float MaxAngularRad { get { return _maxAngular * Mathf.Deg2Rad; } }
    public float MaxAngularDeg { get { return _maxAngular; } }
    public float FleeTreshold { get { return _fleeTreshold; } }
    public float ArriveTime { get { return _arriveTime; } }
    public float Prediction { get { return _prediction; } }
    public float LinearSlowRadius { get { return _linearSlowRadius; } }
    public float LinearTargetRadius { get { return _linearTargetRadius; } }
    public float AngularSlowRadiusRad { get { return _angularSlowRadius * Mathf.Deg2Rad; } }
    public float AngularSlowRadiusDeg { get { return _angularSlowRadius; } }
    public float AngularTargetRadiusRad { get { return _angularTargetRadius * Mathf.Deg2Rad; } }
    public float AngularTargetRadiusDeg { get { return _angularTargetRadius; } }
    public float AvoidanceDistance { get { return _avoidanceDistance; } }
    public float LinearFriction { get { return _linearFriction; } }
    public float AngularFriction { get { return _angularFriction; } }
}
