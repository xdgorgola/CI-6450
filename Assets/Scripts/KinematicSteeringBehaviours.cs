using UnityEngine;
using System;

namespace IA.Steering.Kinematic
{
    public class Seek : BaseSteering
    {
        private StaticMovementData Character { get; init; }
        private StaticMovementData Target { get; init; }


        public override SteeringOutput GetSteering()
        {
            Vector3 vel = (Target.Position - Character.Position).normalized * MoveVars.MaxSpeed;
            Character.Orientation = MathUtils.NewOrientation(Character.Orientation, vel);
            return new SteeringOutput(vel, 0f);
        }


        public Seek(StaticMovementData character, StaticMovementData target, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new System.ArgumentNullException(nameof(character));
            Target = target ?? throw new System.ArgumentNullException(nameof(target));
        }
    }


    public class Flee : BaseSteering
    {
        private StaticMovementData Character { get; init; }
        private StaticMovementData Target { get; init; }


        public override SteeringOutput GetSteering()
        {
            Vector3 vel = Character.Position - Target.Position;
            if (vel.magnitude > MoveVars.FleeTreshold)
                return SteeringOutput.NoSteering;

            vel = vel.normalized * MoveVars.MaxSpeed;
            Character.Orientation = MathUtils.NewOrientation(Character.Orientation, vel);
            return new SteeringOutput(vel, 0f);
        }

        public Flee(StaticMovementData character, StaticMovementData target, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new System.ArgumentNullException(nameof(character));
            Target = target ?? throw new System.ArgumentNullException(nameof(target));
        }
    }


    public class Wander : BaseSteering
    {
        private StaticMovementData Character { get; init; }


        public override SteeringOutput GetSteering()
        {
            Vector3 vel = MathUtils.VectorFromOrientation(Character.Orientation) * MoveVars.MaxSpeed;
            float rotation = MathUtils.RandomBinomial() * MoveVars.MaxRotationRad;
            Character.Orientation += rotation;

            return new SteeringOutput(vel, 0f);
        }

        public Wander(StaticMovementData character, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
        }
    }


    public class Arrive : BaseSteering
    {
        private StaticMovementData Character { get; init; }
        private StaticMovementData Target { get; init; }


        public override SteeringOutput GetSteering()
        {
            Vector3 vel = Target.Position - Character.Position;
            if (vel.magnitude < MoveVars.LinearTargetRadius)
                return SteeringOutput.NoSteering;

            vel /= MoveVars.ArriveTime;
            if (vel.magnitude > MoveVars.MaxSpeed)
                vel = vel.normalized * MoveVars.MaxSpeed;

            Character.Orientation = MathUtils.NewOrientation(Character.Orientation, vel);
            return new SteeringOutput(vel, 0f);
        }

        public Arrive(StaticMovementData character, StaticMovementData target, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }
}
