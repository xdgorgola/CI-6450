using IA.Steering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IA.FSM
{
    public class GetBeerAction : AgentAction
    {
        private AgentMovement _agentMover;
        private CharacterInventory _inv;
        private IEnumerable<ResourceDeposit> _availableDeposit;
        private int _targetAmount;

        public override void Init()
        {
            WorldLocationContext ctx = _agent.LocalCtx;
            _availableDeposit = ctx.GetAvailableDeposits(WorldResource.Beer);
        }

        public override IEnumerator ExecuteAction()
        {
            WorldGrid grid = _agent.LocalCtx?.WorldGrid;
            KinematicMovementData moveData = _agentMover.KinematicData;
            IEnumerable<ResourceDeposit> nearestRes = _availableDeposit.OrderBy((d) => 
                (moveData.Position - (Vector2)d.transform.position).sqrMagnitude)
                .ToList();

            int total = 0;
            int taken = 0;
            foreach (ResourceDeposit dep in nearestRes)
            {
                Vector2 resPos = dep.transform.position;
                var path = grid.CalculatePath(moveData.Position, resPos);
                _agentMover.StartFollowPath(path, false);
                yield return new WaitUntil(() => dep.HasConsumer() || (moveData.Position - resPos).magnitude < 0.5f);
                if (dep.HasConsumer())
                    continue;

                taken = dep.TakeResource(_targetAmount - taken);
                total += taken;
                _inv.AddResource(WorldResource.Beer, taken);
                if (total == _targetAmount)
                    yield break;
            }
        }

        public GetBeerAction(AIAgent agent, int targetAmount) : base(agent)
        {
            if (targetAmount <= 0)
                throw new ArgumentException("[AI.GETBEER.ACTION] Trying to get 0 or negative amount of beer GetBeerAction");

            _targetAmount = targetAmount;

            _agentMover = agent.GetActuator<AgentMovement>();
            if (_agentMover is null)
                throw new ArgumentException("[AI.GETBEER.ACTION] Trying to add agent with no Agent Mover actuator to GetBeerAction");

            _inv = agent.GetFirstSensor<CharacterInventory>();
            if (_inv is null)
            {
                Debug.LogWarning("[AI.DEPOSITRESOURCE.ACTION] Missing CharacterInventory sensor from action");
                return;
            }
        }
    }

    public class ServeBeerAction : AgentAction
    {
        private AgentMovement _agentMover;
        private CharacterInventory _inv;

        public override IEnumerator ExecuteAction()
        {
            WorldGrid grid = _agent.LocalCtx?.WorldGrid;
            KinematicMovementData moveData = _agentMover.KinematicData;
            IEnumerable<ResourceDeposit> emptyTables;
            List<Vector2> path;
            Vector2 resPos;

            while (true)
            {
                if (_inv.GetResourceAmount(WorldResource.Beer) == 0)
                    yield break;

                yield return new WaitUntil(() => _agent.LocalCtx?.GetEmptyBeerTables().Count() > 0);

                emptyTables = _agent.LocalCtx?.GetEmptyBeerTables()
                    .OrderBy((t) => (moveData.Position - (Vector2)t.transform.position).sqrMagnitude)
                    .ToList();

                foreach (ResourceDeposit t in emptyTables)
                {
                    resPos = t.transform.position;
                    path = grid.CalculatePath(moveData.Position, resPos);
                    _agentMover.StartFollowPath(path, false);
                    yield return new WaitUntil(() => t.Amount > 0 || (moveData.Position - resPos).magnitude < 0.5f);
                    if (t.Amount > 0)
                        continue;

                    if (_inv.GetResourceAmount(WorldResource.Beer) == 0)
                        yield break;

                    t.DepositResource(_inv.ConsumeResource(WorldResource.Beer, 1));
                }
            }
        }

        public ServeBeerAction(AIAgent agent) : base(agent)
        {
            _agentMover = agent.GetActuator<AgentMovement>();
            if (_agentMover is null)
                throw new ArgumentException("[AI.GETBEER.ACTION] Trying to add agent with no Agent Mover actuator to GetBeerAction");

            _inv = agent.GetFirstSensor<CharacterInventory>();
            if (_inv is null)
            {
                Debug.LogWarning("[AI.DEPOSITRESOURCE.ACTION] Missing CharacterInventory sensor from action");
                return;
            }
        }
    }
}