namespace IA.FSM
{
    public enum NPCBehaviours
    {
        Miner,
        Drunkard,
        Waiter,
        Debug,
    }

    public static class FSMInstances
    {
        public static StateMachine CreateFSM(NPCBehaviours behaviour, AIAgent agent)
        {
            switch (behaviour)
            {
                case NPCBehaviours.Miner:
                    return MinerFSM(agent);
                case NPCBehaviours.Drunkard:
                    return DrunkardFSM(agent);
                case NPCBehaviours.Waiter:
                    return WaiterFSM(agent);
                case NPCBehaviours.Debug:
                    return TestFSM(agent);
                default:
                    return null;
            }
        }

        public static StateMachine TestFSM(AIAgent agent)
        {
            State idleState = new State("Idle", null, new IdleAction(agent), null);
            State lookingBedState = new State("Looking for bed", null, new LookForBedAction(agent), null);
            State sleepState = new State("Sleep", null, new SleepAction(agent), new WakeUpAction(agent));

            TiredCondition condTired = new TiredCondition(agent);
            RestedCondition condRested = new RestedCondition(agent);
            InBedCondition condInBed = new InBedCondition(agent);
            
            Transition idle2Looking = new Transition(lookingBedState, condTired);
            Transition looking2sleeping = new Transition(sleepState, condInBed);
            Transition sleep2idle = new Transition(idleState, condRested);

            idleState.AddTransition(idle2Looking);
            lookingBedState.AddTransition(looking2sleeping);
            sleepState.AddTransition(sleep2idle);

            return new StateMachine(idleState, null, agent, "TestFSM");
        }

        public static StateMachine MinerFSM(AIAgent agent)
        {
            State lookingVeinState = new State("Looking for vein", null, new LookForResourceAction(agent, WorldResource.Gold), null);
            State miningState = new State("Mining", null, new MiningAction(agent), new StopMiningAction(agent));
            State depositState = new State("Storing", null, new DepositResourceAction(agent, WorldResource.Gold), null);

            InventoryResourceCondition enoughResCondition = new InventoryResourceCondition(agent, WorldResource.Gold, (r) => r >= 10);
            InventoryResourceCondition noResCondition = new InventoryResourceCondition(agent, WorldResource.Gold, (r) => r < 10);
            IsMiningCondition isMiningCondition = new IsMiningCondition(agent);

            Transition lookVein2Mining = new Transition(miningState, isMiningCondition);
            Transition mining2Deposit = new Transition(depositState, enoughResCondition);
            Transition deposit2look = new Transition(lookingVeinState, noResCondition);

            lookingVeinState.AddTransition(lookVein2Mining);
            miningState.AddTransition(mining2Deposit);
            depositState.AddTransition(deposit2look);


            State lookingBedState = new State("Looking for bed", null, new LookForBedAction(agent), null);
            State sleepState = new State("Sleep", null, new SleepAction(agent), new WakeUpAction(agent));

            TiredCondition condTired = new TiredCondition(agent);
            RestedCondition condRested = new RestedCondition(agent);
            InBedCondition condInBed = new InBedCondition(agent);

            Transition any2lookbed = new Transition(lookingBedState, condTired);
            Transition lookBed2sleep = new Transition(sleepState, condInBed);
            Transition sleep2lookRes = new Transition(lookingVeinState, new AndCondition(noResCondition, condRested));
            Transition sleep2deposit = new Transition(depositState, new AndCondition(enoughResCondition, condRested));

            lookingBedState.AddTransition(lookBed2sleep);
            sleepState.AddTransition(sleep2lookRes);
            sleepState.AddTransition(sleep2deposit);


            return new StateMachine(lookingVeinState, new() { any2lookbed }, agent, "MinerFSM");
        }

        public static StateMachine DrunkardFSM(AIAgent agent)
        {
            State lookingTableState = new State("Looking for table", null, new LookForBeerTableAction(agent), null);
            State drinkingState = new State("Drinking beer", null, new DrinkingAction(agent), new StopDrinkingAction(agent));
            State drunkState = new State("Drunk", null, new DrunkAction(agent), new SoberUpAction(agent));

            Condition isDrinkingCond = new IsDrinkingCondition(agent);
            Condition isDrunkCond = new IsDrunkCondition(agent);
            Condition soberedUpCond = new AndCondition(new NotCondition(isDrinkingCond), new NotCondition(isDrunkCond));

            Transition lookingTable2drinking = new Transition(drinkingState, isDrinkingCond);
            Transition drinking2drunk = new Transition(drunkState, isDrunkCond);
            Transition drunk2lookingTable = new Transition(lookingTableState, soberedUpCond);

            lookingTableState.AddTransition(lookingTable2drinking);
            drinkingState.AddTransition(drinking2drunk);
            drunkState.AddTransition(drunk2lookingTable);

            State lookingBedState = new State("Looking for bed", null, new LookForBedAction(agent), null);
            State sleepState = new State("Sleep", null, new SleepAction(agent), new WakeUpAction(agent));
            
            TiredCondition condTired = new TiredCondition(agent);
            RestedCondition condRested = new RestedCondition(agent);
            InBedCondition condInBed = new InBedCondition(agent);
            
            Transition any2lookbed = new Transition(lookingBedState, condTired);
            Transition lookBed2sleep = new Transition(sleepState, condInBed);
            Transition sleep2drunk = new Transition(drunkState, new AndCondition(isDrunkCond, condRested));
            Transition sleep2lookinTable = new Transition(lookingTableState, new AndCondition(soberedUpCond, condRested));
            
            lookingBedState.AddTransition(lookBed2sleep);
            sleepState.AddTransition(sleep2drunk);
            sleepState.AddTransition(sleep2lookinTable);

            return new StateMachine(lookingTableState, new() { any2lookbed }, agent, "Drunkard FSM");
        }

        public static StateMachine WaiterFSM(AIAgent agent)
        {
            State idleState = new State("Idle", null, null, null);
            State gettingBeerState = new State("Getting beer", null, new GetBeerAction(agent, 10), null);
            State servingBeerState = new State("Serving beer", null, new ServeBeerAction(agent), null);

            Condition hasNoBeerCond = new InventoryResourceCondition(agent, WorldResource.Beer, (b) => b == 0);
            Condition hasBeersCond = new NotCondition(hasNoBeerCond);
            Condition has10beerCond = new InventoryResourceCondition(agent, WorldResource.Beer, (b) => b >= 10); ;
            Condition emptyTableCond = new EmptyTablesCondition(agent);
            Condition fullTablesCond = new NotCondition(emptyTableCond);

            Transition idle2getting = new Transition(gettingBeerState, hasNoBeerCond);
            Transition idle2serving = new Transition(servingBeerState, new AndCondition(hasBeersCond, emptyTableCond));
            Transition getting2idle = new Transition(idleState, new AndCondition(has10beerCond, fullTablesCond));
            Transition getting2serving = new Transition(servingBeerState, new AndCondition(has10beerCond, emptyTableCond));
            Transition serving2getting = new Transition(gettingBeerState, hasNoBeerCond);
            Transition serving2idle = new Transition(idleState, new AndCondition(hasBeersCond, fullTablesCond));

            idleState.AddTransition(idle2getting);
            idleState.AddTransition(idle2serving);
            gettingBeerState.AddTransition(getting2idle);
            gettingBeerState.AddTransition(getting2serving);
            servingBeerState.AddTransition(serving2idle);
            servingBeerState.AddTransition(serving2getting);

            State lookingBedState = new State("Looking for bed", null, new LookForBedAction(agent), null);
            State sleepState = new State("Sleep", null, new SleepAction(agent), new WakeUpAction(agent));
            
            TiredCondition condTired = new TiredCondition(agent);
            RestedCondition condRested = new RestedCondition(agent);
            InBedCondition condInBed = new InBedCondition(agent);
            
            Transition any2lookbed = new Transition(lookingBedState, condTired);
            Transition lookBed2sleep = new Transition(sleepState, condInBed);
            Transition sleep2Idle = new Transition(idleState, new AndCondition(new AndCondition(hasBeersCond, fullTablesCond), condRested));
            Transition sleep2getting = new Transition(gettingBeerState, new AndCondition(hasNoBeerCond, condRested));
            Transition sleep2serving = new Transition(servingBeerState, new AndCondition(new AndCondition(hasBeersCond, emptyTableCond), condRested));
            
            lookingBedState.AddTransition(lookBed2sleep);
            sleepState.AddTransition(sleep2Idle);
            sleepState.AddTransition(sleep2getting);
            sleepState.AddTransition(sleep2serving);

            return new StateMachine(idleState, new() { any2lookbed }, agent, "Waiter FSM");
        }
    }
}
