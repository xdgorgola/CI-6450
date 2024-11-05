using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace IA.FSM
{
    public class StateMachine
    {
        public string Name { get; init; } = "FSM";
        private AIAgent _owner;
        private State _initialState;
        private State _currentState;
        
        private List<Transition> _globalTransitions = null;
        private Transition _lastTransition = null;
        private bool _inTransition = false;

        private Coroutine _currentStateRoutine = null;

        public ref readonly State CurrentState
        {
            get { return ref _currentState; }
        }

        public bool InTransition
        {
            get { return _inTransition; }
        }

        public void Tick()
        {
            if (_inTransition)
                return;

            Transition triggered = null;
            if (_globalTransitions is not null)
            {
                triggered = CheckTransitions(_globalTransitions);
                if (_lastTransition == triggered) // To prevent looping back to the same transition after taking a global one
                    triggered = null;

                if (triggered is not null)
                {
                    Debug.Log("Triggered global transition");
                    Debug.Log(triggered == _lastTransition);
                }
            }

            if (triggered is null)
                triggered = CheckTransitions(_currentState.Transitions);

            if (triggered is null)
                return;

            ExecuteTransition(triggered);
            _lastTransition = triggered;
        }


        private void ExecuteTransition(Transition transition)
        {
            if (_currentStateRoutine is not null)
                _owner.StopCoroutine(_currentStateRoutine);

            _owner.StartCoroutine(TransitionRoutine(transition));
        }


        private Transition CheckTransitions(IEnumerable<Transition> transitions)
        {
            foreach (Transition t in transitions)
            {
                if (!t.IsTriggered())
                    continue;

                return t;
            }

            return null;
        }


        public void StartFSM()
        {
            _owner.StartCoroutine(StartFSMCo());
        }


        public void StopFSM()
        {
            _owner.StopCoroutine(_currentStateRoutine);
        }

        private IEnumerator StartFSMCo()
        {
            _currentStateRoutine = _owner.StartCoroutine(_currentState.ExecuteState());
            yield return _currentStateRoutine;
        }

        private IEnumerator TransitionRoutine(Transition transition)
        {
            Debug.Log($"[AI] Exiting state {_currentState.Name}");
            _inTransition = true;
            _currentStateRoutine = _owner.StartCoroutine(_currentState.ExitState());

            yield return _currentStateRoutine;

            Debug.Log($"[AI] Entry to state {transition.Target.Name}");
            _currentState = transition.Target;
            _currentStateRoutine = _owner.StartCoroutine(_currentState.EntryState());

            yield return _currentStateRoutine;

            Debug.Log($"[AI] In state {_currentState.Name}");
            _inTransition = false;
            _currentStateRoutine = _owner.StartCoroutine(_currentState.ExecuteState());

            yield return _currentStateRoutine;
        }

        public StateMachine(State initialState, List<Transition> globalTransitions, AIAgent owner, string name)
        {
            _initialState = initialState ?? throw new ArgumentNullException(nameof(initialState));
            _currentState = _initialState;
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _globalTransitions = globalTransitions;
            Name = name ?? "FSM";
        }
    }


    public class Transition
    {
        public State Target { get; private set; }
        private Condition _condition;

        public bool IsTriggered() =>
            _condition.Test();

        public Transition(State target, Condition condition)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            _condition = condition ?? throw new ArgumentNullException(nameof(_condition));
        }
    }

    public abstract class Condition
    {
        public abstract bool Test();
    }


    public class State
    {
        public string Name { get; init; } = "FSM";
        public LinkedList<Transition> Transitions { get; private set; } = new();
        
        private AgentAction _entryAction = null;
        private AgentAction _stateAction = null;
        private AgentAction _exitAction = null;

        public void AddTransition(Transition transition)
        {
            Transitions.AddLast(transition);
        }

        public IEnumerator EntryState()
        {
            if (_entryAction is not null)
            {
                _entryAction.Init();
                yield return _entryAction.ExecuteAction();
            }
        }

        public IEnumerator ExecuteState()
        {
            if (_stateAction is not null)
            {
                _stateAction.Init();
                yield return _stateAction.ExecuteAction();
            }
        }


        public IEnumerator ExitState()
        {
            if (_exitAction is not null)
            {
                _exitAction.Init();
                yield return _exitAction.ExecuteAction();
            }
        }

        public State(string name, AgentAction entryAction, AgentAction stateAction, AgentAction exitAction)
        {
            Name = name ?? "FSM";
            _entryAction = entryAction;
            _stateAction = stateAction;
            _exitAction = exitAction;

            if (_entryAction is null && _stateAction is null && _exitAction is null)
                Debug.LogWarning($"[AI] State {name} has no actions");
        }
    }
}