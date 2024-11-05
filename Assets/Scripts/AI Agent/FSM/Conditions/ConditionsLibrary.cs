using System;
using System.Linq;

namespace IA.FSM
{
    public class TiredCondition : Condition
    {
        private AIAgent _agent;
        private CharacterEnergy _energy;

        public override bool Test()
        {
            return _energy.Energy == 0f && _energy.OccupiedBed is null;
        }

        public TiredCondition(AIAgent agent)
        {
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
            _energy = agent.GetFirstSensor<CharacterEnergy>();
            if (_energy is null)
                throw new ArgumentException("[AI] Trying to add agent with no CharacterEnergy sensor to TiredCondition");
        }
    }

    public class InBedCondition : Condition
    {
        private AIAgent _agent;
        private CharacterEnergy _energy;

        public override bool Test()
        {
            return _energy.OccupiedBed is not null;
        }

        public InBedCondition(AIAgent agent)
        {
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
            _energy = agent.GetFirstSensor<CharacterEnergy>();
            if (_energy is null)
                throw new ArgumentException("[AI] Trying to add agent with no CharacterEnergy sensor to InBedCondition Condition");
        }
    }

    public class RestedCondition : Condition
    {
        private CharacterEnergy _energy;

        public override bool Test()
        {
            return _energy.IsRested();
        }

        public RestedCondition(AIAgent agent)
        {
            if (agent is null)
                throw new ArgumentNullException(nameof(agent));

            _energy = agent.GetFirstSensor<CharacterEnergy>();
            if (_energy is null)
                throw new ArgumentException("[AI] Trying to add agent with no CharacterEnergy sensor to RestedCondition");
        }
    }

    public class MissingWorldContextCondition : Condition
    {
        private AIAgent _agent;

        public override bool Test()
        {
            return _agent.LocalCtx is null;
        }

        public MissingWorldContextCondition(AIAgent agent)
        {
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        }
    }

    public class HasWorldContextCondition : Condition
    {
        private AIAgent _agent;

        public override bool Test()
        {
            return _agent.LocalCtx is not null;
        }

        public HasWorldContextCondition(AIAgent agent)
        {
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        }
    }

    public class InventoryResourceCondition : Condition
    {
        private CharacterInventory _agentInv;
        private Predicate<float> _cond;
        private WorldResource _res;

        public override bool Test()
        {
            return _cond(_agentInv.GetResourceAmount(_res));
        }

        public InventoryResourceCondition(AIAgent agent, WorldResource res, Predicate<float> cond)
        {


            _cond = cond ?? throw new ArgumentNullException(nameof(cond));
            _agentInv = agent.GetFirstSensor<CharacterInventory>();
            if (_agentInv is null)
            {
                Debug.LogWarning("[AI] Missing Character Inventory sensor for InventoryResourceCondition");
                return;
            }

            _res = res;
        }
    }

    public class IsMiningCondition : Condition
    {
        private CharacterMining _mining;

        public override bool Test()
        {
            return _mining.OwnedVein is not null;
        }

        public IsMiningCondition(AIAgent agent)
        {
            if (agent is null)
                throw new ArgumentNullException(nameof(agent));

            _mining = agent.GetActuator<CharacterMining>();
            if (_mining is null)
                throw new ArgumentException("[AI] Trying to add agent with no CharacterMining sensor to IsMiningCondition Condition");
        }
    }

    public class IsDrinkingCondition : Condition
    {
        private CharacterDrunkness _drunkness;

        public override bool Test()
        {
            return _drunkness.IsDrinking();
        }

        public IsDrinkingCondition(AIAgent agent)
        {
            if (agent is null)
                throw new ArgumentNullException(nameof(agent));

            _drunkness = agent.GetActuator<CharacterDrunkness>();
            if (_drunkness is null)
                throw new ArgumentException("[AI] Trying to add agent with no CharacterDrunkness sensor to IsDrunkCondition");
        }
    }

    public class IsDrunkCondition : Condition
    {
        private CharacterDrunkness _drunkness;

        public override bool Test()
        {
            return _drunkness.IsDrunk();
        }

        public IsDrunkCondition(AIAgent agent)
        {
            if (agent is null)
                throw new ArgumentNullException(nameof(agent));

            _drunkness = agent.GetActuator<CharacterDrunkness>();
            if (_drunkness is null)
                throw new ArgumentException("[AI] Trying to add agent with no CharacterDrunkness sensor to IsDrunkCondition");
        }
    }

    public class EmptyTablesCondition : Condition
    {
        private AIAgent _agent;

        public override bool Test()
        {
            if (_agent.LocalCtx is null)
                return false;

            return _agent.LocalCtx?.GetEmptyBeerTables().Count() > 0;
        }

        public EmptyTablesCondition(AIAgent agent)
        {
            _agent = agent ?? throw new ArgumentNullException(nameof(agent));
        }
    }
}