using IA.Steering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IA.FSM
{
    public class LookForBeerTableAction : AgentAction
    {
        private AgentMovement _agentMover;
        private CharacterDrunkness _drunkness;

        public override IEnumerator ExecuteAction()
        {
            WorldGrid grid = _agent.LocalCtx?.WorldGrid;
            KinematicMovementData moveData = _agentMover.KinematicData;
            IEnumerable<ResourceDeposit> nearestTable;
            List<Vector2> path;
            Vector2 tablePos;

            while (true)
            {
                yield return new WaitUntil(() => _agent.LocalCtx?.GetAvailableBeerTables().Count() > 0);
                nearestTable = _agent.LocalCtx?.GetAvailableBeerTables()
                    .OrderBy((d) => (moveData.Position - (Vector2)d.transform.position).sqrMagnitude)
                    .ToList();

                foreach (ResourceDeposit dep in nearestTable)
                {
                    tablePos = dep.transform.position;
                    path = grid.CalculatePath(moveData.Position, tablePos);
                    _agentMover.StartFollowPath(path, false);
                    yield return new WaitUntil(() => dep.HasConsumer() || (moveData.Position - tablePos).magnitude < 0.5f);
                    if (dep.HasConsumer())
                        continue;

                    _drunkness.OccupyBeerDeposit(dep);
                    yield break;
                }
            }
        }

        public LookForBeerTableAction(AIAgent agent) : base(agent)
        {
            _agentMover = agent.GetActuator<AgentMovement>();
            if (_agentMover is null)
                throw new ArgumentException("[AI.LOOKFORBEERTABLEE.ACTION] Trying to add agent with no Agent Mover actuator to LookForBeerTable");

            _drunkness = agent.GetActuator<CharacterDrunkness>();
            if (_drunkness is null)
                throw new ArgumentException("[AI.LOOKFORBEERTABLEE.ACTION] Trying to add agent with no Character Drunkness sensor to LookForBeerTable");
        }
    }

    public class DrinkingAction : AgentAction
    {
        private CharacterDrunkness _drunkness;

        public override IEnumerator ExecuteAction()
        {
            // Actually do nothing?
            Debug.Log("[AI.MINING.ACTION] *drinking*");
            yield break;
        }

        public DrinkingAction(AIAgent agent) : base(agent)
        {
            _drunkness = agent.GetActuator<CharacterDrunkness>();
            if (_drunkness is null)
                throw new ArgumentException("[AI.DRINKING.ACTION] Trying to add agent with no CharacterDrunkness actuator to DrinkingAction");
        }
    }

    public class StopDrinkingAction : AgentAction
    {
        private CharacterDrunkness _drunkness;

        public override IEnumerator ExecuteAction()
        {
            Debug.Log("[AI.STOPDRINKING.ACTION] Stopping mining");
            _drunkness.DeOccupyBeerDeposit();
            yield break;
        }

        public StopDrinkingAction(AIAgent agent) : base(agent)
        {
            _drunkness = agent.GetActuator<CharacterDrunkness>();
            if (_drunkness is null)
                throw new ArgumentException("[AI.STOPDRINKING.ACTION] Trying to add agent with no CharacterDrunkness actuator to StopDrinkingAction");
        }
    }

    public class DrunkAction : AgentAction
    {
        private AgentMovement _agentMover;


        public override IEnumerator ExecuteAction()
        {
            _agentMover.EnableDynamicWander();
            yield break;
        }

        public DrunkAction(AIAgent agent) : base(agent)
        {
            _agentMover = agent.GetActuator<AgentMovement>();
            if (_agentMover is null)
                throw new ArgumentException("[AI.DEPOSITRESOURCE.ACTION] Trying to add agent with no Agent Mover actuator to DepositResourceAction");
        }
    }

    public class SoberUpAction : AgentAction
    {
        private AgentMovement _agentMover;


        public override IEnumerator ExecuteAction()
        {
            _agentMover.ClearBaseMovement();
            yield break;
        }

        public SoberUpAction(AIAgent agent) : base(agent)
        {
            _agentMover = agent.GetActuator<AgentMovement>();
            if (_agentMover is null)
                throw new ArgumentException("[AI.DEPOSITRESOURCE.ACTION] Trying to add agent with no Agent Mover actuator to DepositResourceAction");
        }
    }
}