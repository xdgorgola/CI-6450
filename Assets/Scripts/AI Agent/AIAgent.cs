using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

using IA.FSM;

public class AIAgent : MonoBehaviour
{
    [SerializeField]
    private List<Component> _sensorList = new();
    [SerializeField]
    private List<Component> _actuatorList = new();

    private Dictionary<Type, Component> _actuators = new();
    private Dictionary<Type, LinkedList<Component>> _sensors = new();

    [SerializeField]
    private WorldLocationContext _localCtx = null;

    [SerializeField]
    private NPCBehaviours _behaviour = NPCBehaviours.Miner;
    private StateMachine _fsm = null;

    public ref readonly WorldLocationContext LocalCtx
    {
        get { return ref _localCtx; }
    }

    public ref readonly StateMachine FSM
    {
        get { return ref _fsm; }
    }

    private void Awake()
    {
        foreach (Component sensor in _sensorList)
        {
            Type type = sensor.GetType();
            if (!_sensors.ContainsKey(type))
                _sensors.Add(type, new LinkedList<Component>());

            _sensors[type].AddLast(sensor);
        }

        foreach (Component actuator in _actuatorList)
        {
            Type type = actuator.GetType();
            if (_actuators.ContainsKey(type))
            {
                Debug.LogWarning($"[AI] Trying to add multiple {actuator.GetType()} actuators to agent {name}");
                continue;
            }

            _actuators.Add(type, actuator);
        }
    }

    private void Start()
    {
        GetActuator<AgentMovement>().ToggleWallAvoidance();
        _fsm = FSMInstances.CreateFSM(_behaviour, this);
        if (_fsm is null)
        {
            Debug.LogWarning($"[AI] AI Agent {name} has no FSM");
            return;
        }

        _fsm.StartFSM();
    }

    private void Update()
    {
        if (_fsm is null)
        {
            Debug.LogWarning($"[AI] AI Agent {name} has no FSM");
            return;
        }

        _fsm.Tick();
    }

    public IEnumerable<T> GetSensors<T>() where T : Component
    {
        Type ttype = typeof(T);
        if (!_sensors.ContainsKey(ttype))
            return Enumerable.Empty<T>();

        return _sensors[ttype].Cast<T>();
    }

    public T GetFirstSensor<T>() where T: Component
    {
        Type ttype = typeof(T);
        if (!_sensors.ContainsKey(ttype))
            return null;

        return _sensors[ttype].First.Value as T;
    }

    public T GetActuator<T>() where T : Component
    {
        Type ttype = typeof(T);
        if (!_actuators.ContainsKey(ttype))
            return null;

        return _actuators[ttype] as T;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("WorldContext"))
            return;

        if (_localCtx is not null)
        {
            Debug.LogWarning("[AI] Entering to multiple world contexts at the same time");
            return;
        }

        _localCtx = collision.GetComponent<WorldLocationContext>();
    }


    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("WorldContext"))
            return;

        if (_localCtx is null)
        {
            Debug.LogWarning("[AI] Exiting a world context, but the current context is null");
            return;
        }

        if (_localCtx.gameObject == collision.gameObject)
            _localCtx = null;
    }
}