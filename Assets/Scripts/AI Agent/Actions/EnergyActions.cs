using IA.Steering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IA.FSM
{
    public class IdleAction : AgentAction
    {
        private CharacterEnergy _energy;

        public override IEnumerator ExecuteAction()
        {
            while (true)
            {
                yield return new WaitForSeconds(1.0f);
                _energy.ConsumeEnergy(1f);
            }
        }

        public IdleAction(AIAgent agent) : base(agent)
        {
            _energy = agent.GetFirstSensor<CharacterEnergy>();
            if (_energy is null)
                throw new ArgumentException("[AI] Trying to add agent with no CharacterEnergy sensor to Tiredness Condition");
        }
    }

    public class LookForBedAction : AgentAction
    {
        private AgentMovement _agentMover;

        public override IEnumerator ExecuteAction()
        {
            WorldGrid grid = _agent.LocalCtx?.WorldGrid;
            KinematicMovementData moveData = _agentMover.KinematicData;
            IEnumerable<CharacterBed> nearestBeds;
            List<Vector2> path;
            Vector2 bedPos;

            while (true)
            {
                yield return new WaitUntil(() => _agent.LocalCtx?.GetAvailableBeds().Count() > 0);

                nearestBeds = _agent.LocalCtx?.GetAvailableBeds()
                    .OrderBy((b) => (moveData.Position - (Vector2)b.transform.position).sqrMagnitude)
                    .ToList();

                Debug.Log($"[AI.LOOKFORBED.ACTION] Nearest bed count {nearestBeds.Count()}");
                foreach (CharacterBed bed in nearestBeds)
                {
                    bedPos = bed.transform.position;
                    path = grid.CalculatePath(moveData.Position, bedPos);
                    _agentMover.StartFollowPath(path, false);

                    yield return new WaitUntil(() => !bed.Available || (moveData.Position - bedPos).magnitude < 0.5f);
                    
                    if (!bed.Available)
                        continue;

                    Debug.Log($"[AI.LOOKFORBED.ACTION] Occupying bed {bed.name}");
                    _agent.GetFirstSensor<CharacterEnergy>().OccupyBed(bed);
                    yield break;
                }
            }
        }

        public LookForBedAction(AIAgent agent) : base(agent)
        {
            _agentMover = agent.GetActuator<AgentMovement>();
            if (_agentMover is null)
                throw new ArgumentException("[AI.LOOKFORBED.ACTION] Trying to add agent with no Agent Mover actuator to LookForBedAction");
        }
    }

    public class SleepAction : AgentAction
    {
        private CharacterEnergy _energy;

        public override IEnumerator ExecuteAction()
        {
            //_energy.StartRegeneration();
            // Actually do nothing?
            Debug.Log("[AI.SLEEPING.ACTION] *snoring*");
            yield break;
        }

        public SleepAction(AIAgent agent) : base(agent)
        {
            _energy = agent.GetFirstSensor<CharacterEnergy>();
            if (_energy is null)
                throw new ArgumentException("[AI.SLEEP.ACTION] Trying to add agent with no CharacterEnergy actuator to SleepAction");
        }
    }


    public class WakeUpAction : AgentAction
    {
        private CharacterEnergy _energy;

        public override IEnumerator ExecuteAction()
        {
            Debug.Log("[AI.WAKEUP.ACTION] Deoccupying bed!");
            _energy.DeOccupyBed();
            yield break;
        }

        public WakeUpAction(AIAgent agent) : base(agent)
        {
            _energy = agent.GetFirstSensor<CharacterEnergy>();
            if (_energy is null)
                throw new ArgumentException("[AI.WAKEUP.ACTION] Trying to add agent with no CharacterEnergy actuator to WakeUpAction");
        }
    }
}
