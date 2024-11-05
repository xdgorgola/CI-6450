using IA.Steering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IA.FSM
{
    public class LookForResourceAction : AgentAction
    {
        private WorldResource _resType;
        private AgentMovement _agentMover;
        private CharacterMining _mining;

        public override void Init()
        {
            WorldLocationContext ctx = _agent.LocalCtx;
        }

        public override IEnumerator ExecuteAction()
        {
            WorldGrid grid = _agent.LocalCtx?.WorldGrid;
            KinematicMovementData moveData = _agentMover.KinematicData;
            IEnumerable<RegeneratingResource> nearestRes;
            List<Vector2> path;
            Vector2 resPos;

            while (true)
            {
                yield return new WaitUntil(() => _agent.LocalCtx?.GetAvailableRegeneratingResources(_resType).Count() > 0);
                nearestRes = _agent.LocalCtx?.GetAvailableRegeneratingResources(_resType)
                    .OrderBy((r) => (moveData.Position - (Vector2)r.transform.position).sqrMagnitude)
                    .ToList();

                foreach (RegeneratingResource res in nearestRes)
                {
                    resPos = res.transform.position;
                    path = grid.CalculatePath(moveData.Position, resPos);
                    _agentMover.StartFollowPath(path, false);
                    yield return new WaitUntil(() => !res.Available || (moveData.Position - resPos).magnitude < 0.5f);
                    if (!res.Available)
                        continue;

                    _mining.StartMining(res);
                    yield break;
                }
            }
        }

        public LookForResourceAction(AIAgent agent, WorldResource res) : base(agent)
        {
            _resType = res;
            _agentMover = agent.GetActuator<AgentMovement>();
            if (_agentMover is null)
                throw new ArgumentException("[AI.LOOKFORRESOURCE.ACTION] Trying to add agent with no Agent Mover actuator to LookForResource");

            _mining = agent.GetActuator<CharacterMining>();
            if (_mining is null)
                throw new ArgumentException("[AI.LOOKFORRESOURCE.ACTION] Trying to add agent with no CharacterMining actuator to LookForResource");
        }
    }

    public class MiningAction : AgentAction
    {
        private CharacterMining _mining;

        public override IEnumerator ExecuteAction()
        {
            // Actually do nothing?
            Debug.Log("[AI.MINING.ACTION] *mining*");
            yield break;
        }

        public MiningAction(AIAgent agent) : base(agent)
        {
            _mining = agent.GetActuator<CharacterMining>();
            if (_mining is null)
                throw new ArgumentException("[AI.MINING.ACTION] Trying to add agent with no CharacterMining actuator to MiningAction");
        }
    }

    public class StopMiningAction : AgentAction
    {
        private CharacterMining _mining;

        public override IEnumerator ExecuteAction()
        {
            Debug.Log("[AI.STOPMINING.ACTION] Stopping mining");
            _mining.StopMining();
            yield break;
        }

        public StopMiningAction(AIAgent agent) : base(agent)
        {
            _mining = agent.GetActuator<CharacterMining>();
            if (_mining is null)
                throw new ArgumentException("[AI.LOOKFORRESOURCE.ACTION] Trying to add agent with no CharacterMining actuator to LookForResource");
        }
    }

    public class DepositResourceAction : AgentAction
    {
        private CharacterInventory _inv;
        private AgentMovement _agentMover;

        private WorldResource _resType;
        private IEnumerable<ResourceDeposit> _deposits;

        public override void Init()
        {
            WorldLocationContext ctx = _agent.LocalCtx;
            _deposits = ctx.GetDeposits(_resType);
        }

        public override IEnumerator ExecuteAction()
        {
            WorldGrid grid = _agent.LocalCtx?.WorldGrid;
            KinematicMovementData moveData = _agentMover.KinematicData;
            IEnumerable<ResourceDeposit> nearestDepos = _deposits
                .OrderBy((d) => (moveData.Position - (Vector2)d.transform.position).sqrMagnitude)
                .ToList();

            foreach (ResourceDeposit d in nearestDepos)
            {
                Vector2 depPos = d.transform.position;
                var path = grid.CalculatePath(moveData.Position, depPos);
                _agentMover.StartFollowPath(path, false);
                yield return new WaitUntil(() => (moveData.Position - depPos).magnitude < 0.5f);

                d.DepositResource(_inv.ConsumeResource(_resType, _inv.GetResourceAmount(_resType)));
                yield break;
            }
        }

        public DepositResourceAction(AIAgent agent, WorldResource resType) : base(agent)
        {
            _resType = resType;
            _inv = agent.GetFirstSensor<CharacterInventory>();
            if (_inv is null)
            {
                Debug.LogWarning("[AI.DEPOSITRESOURCE.ACTION] Missing CharacterInventory sensor from action");
                return;
            }

            _agentMover = agent.GetActuator<AgentMovement>();
            if (_agentMover is null)
                throw new ArgumentException("[AI.DEPOSITRESOURCE.ACTION] Trying to add agent with no Agent Mover actuator to DepositResourceAction");
        }
    }
}