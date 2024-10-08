using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.Steering.Dynamic
{
    public class Seek : BaseSteering
    {
        protected KinematicMovementData Character { get; init; }
        protected StaticMovementData Target { get; init; }

        public override SteeringOutput GetSteering()
        {
            Vector3 vel = (Target.Position - Character.Position).normalized * MoveVars.MaxAcceleration;
            return new SteeringOutput(vel, 0f);
        }


        public Seek(KinematicMovementData character, StaticMovementData target, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }


    public class Flee : BaseSteering
    {
        protected KinematicMovementData Character { get; init; }
        protected StaticMovementData Target { get; init; }


        public override SteeringOutput GetSteering()
        {
            Vector3 vel = Character.Position - Target.Position;
            if (vel.magnitude > MoveVars.FleeTreshold)
                return SteeringOutput.NoSteering;
                //return new SteeringOutput(-Character.Velocity * 1.25f, 0f);

            vel = vel.normalized * MoveVars.MaxAcceleration;
            return new SteeringOutput(vel, 0f);
        }


        public Flee(KinematicMovementData character, StaticMovementData target, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }


    public class Arrive : BaseSteering
    {
        private KinematicMovementData Character { get; init; }
        private KinematicMovementData Target { get; init; }

        public override SteeringOutput GetSteering()
        {
            Vector2 dir = Target.Position - Character.Position;
            float dist = dir.magnitude;
            if (dist < MoveVars.LinearTargetRadius)
                return SteeringOutput.NoSteering;

            float targetSpeed = MoveVars.MaxSpeed;
            if (dist <= MoveVars.LinearSlowRadius)
                targetSpeed = MoveVars.MaxSpeed * dist / MoveVars.LinearSlowRadius;

            Vector2 targetVel = dir.normalized * targetSpeed;
            Vector2 vel = (targetVel - Character.Velocity) / MoveVars.ArriveTime;
            if (vel.magnitude > MoveVars.MaxAcceleration)
                vel = vel.normalized * MoveVars.MaxAcceleration;

            return new SteeringOutput(vel, 0f);
        }


        public Arrive(KinematicMovementData character, KinematicMovementData target, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }


    public class Align : BaseSteering
    {
        protected KinematicMovementData Character { get; init; }
        protected StaticMovementData Target { get; init; }


        public override SteeringOutput GetSteering()
        {
            float rotation = Mathf.DeltaAngle(Character.Orientation * Mathf.Rad2Deg, Target.Orientation * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            float rotationSize = Mathf.Abs(rotation);

            if (rotationSize < MoveVars.AngularTargetRadiusRad)
                return SteeringOutput.NoSteering;

            float targetRotation = MoveVars.MaxRotationDeg;
            if (rotationSize <= MoveVars.AngularSlowRadiusRad)
                targetRotation = MoveVars.MaxRotationRad * rotationSize / MoveVars.AngularSlowRadiusRad;

            targetRotation *= rotation / rotationSize;

            float angular = (targetRotation - Character.Rotation) / MoveVars.ArriveTime;
            float angularAccel = Mathf.Abs(angular);
            if (angularAccel > MoveVars.MaxAngularRad)
            {
                angular /= angularAccel;
                angular *= MoveVars.MaxAngularRad;
            }

            return new SteeringOutput(Vector2.zero, angular);
        }


        public Align(KinematicMovementData character, StaticMovementData target, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }


    public class VelocityMatch : BaseSteering
    {
        private KinematicMovementData Character { get; init; }
        private KinematicMovementData Target { get; init; }


        public override SteeringOutput GetSteering()
        {
            Vector2 linear = (Target.Velocity - Character.Velocity) / MoveVars.ArriveTime;
            if (linear.magnitude > MoveVars.MaxAcceleration)
                linear = linear.normalized * MoveVars.MaxAcceleration;

            return new SteeringOutput(linear, 0f);
        }

        public VelocityMatch(KinematicMovementData character, KinematicMovementData target, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }


    public class Pursue : Seek
    {
        new protected KinematicMovementData Target { get; init; }


        public override SteeringOutput GetSteering()
        {
            Vector3 dir = Target.Position - Character.Position;
            float dist = dir.magnitude;
            float speed = Character.Velocity.magnitude;
            float prediction = speed <= dist / MoveVars.Prediction ? MoveVars.Prediction : dist / speed;

            Vector2 predictedPos = Target.Position + Target.Velocity * prediction;
            base.Target.Position = predictedPos;


#if UNITY_EDITOR
            Debug.DrawCross(predictedPos, 0.25f, Color.white);
#endif
            return base.GetSteering();
        }

        public Pursue(KinematicMovementData character,
                      KinematicMovementData target,
                      AgentMovementVars moveVars) : base(character, new(), moveVars)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }


    public class Evade : Flee
    {
        new protected KinematicMovementData Target { get; init; }

        public override SteeringOutput GetSteering()
        {
            Vector3 dir = Target.Position - Character.Position;
            float dist = dir.magnitude;
            float speed = Character.Velocity.magnitude;
            float prediction = speed <= dist / MoveVars.Prediction ? MoveVars.Prediction : dist / speed;

            Vector2 predictedPos = Target.Position + Target.Velocity * prediction;
            base.Target.Position = predictedPos;

#if UNITY_EDITOR
            Debug.DrawCross(predictedPos, 0.25f, Color.black);
#endif
            return base.GetSteering();
        }

        public Evade(KinematicMovementData character,KinematicMovementData target, AgentMovementVars moveVars) : base(character, new(), moveVars)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }


    public class Face : Align
    {
        new protected KinematicMovementData Target { get; init; }

        public override SteeringOutput GetSteering()
        {
            Vector2 dir = Target.Position - Character.Position;
            if (dir.sqrMagnitude == 0)
                return SteeringOutput.NoSteering;

            base.Target.Orientation = MathUtils.NewOrientation(Character.Orientation, dir);
            return base.GetSteering();
        }

        public Face(KinematicMovementData character,
                    KinematicMovementData target,
                    AgentMovementVars moveVars) : base(character, new(), moveVars)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
        }
    }

    public class LookVelocity : Align
    {
        public override SteeringOutput GetSteering()
        {
            if (Character.Velocity.sqrMagnitude == 0)
                return SteeringOutput.NoSteering;

            Target.Orientation = MathUtils.NewOrientation(Character.Orientation, Character.Velocity);
            return base.GetSteering();
        }

        public LookVelocity(KinematicMovementData character, AgentMovementVars moveVars): base(character, new(), moveVars) {}
    }

    public class Wander : Face
    {
        private readonly float _wanderOffset;
        private readonly float _wanderRadius;
        private readonly float _wanderRate;
        private float _wanderOrientation = 0f;
    
        public override SteeringOutput GetSteering()
        {
            _wanderOrientation += MathUtils.RandomBinomial() * _wanderRate;
            _wanderOrientation = MathUtils.WrapOrientation(_wanderOrientation).radOr;
            float targetOrientation = _wanderOrientation + Character.Orientation;
            Target.Position = Character.Position + _wanderOffset * MathUtils.VectorFromOrientation(targetOrientation);
            Target.Position += _wanderRadius * MathUtils.VectorFromOrientation(targetOrientation);

#if UNITY_EDITOR
            Debug.DrawCircle(Character.Position + _wanderOffset * MathUtils.VectorFromOrientation(targetOrientation), _wanderRadius, 32, Color.blue);
            Debug.DrawCross(Target.Position, 0.25f, Color.black);
#endif

            SteeringOutput res = base.GetSteering();
            return new SteeringOutput(MoveVars.MaxAcceleration * MathUtils.VectorFromOrientation(Character.Orientation), res.angular);
        }

        public Wander(KinematicMovementData character,
                      AgentMovementVars moveVars,
                      float wanderOffset,
                      float wanderRadius,
                      float wanderRate, float startOrientation = 0f) : base(character, new(), moveVars)
        {
            _wanderOffset = wanderOffset;
            _wanderRadius = wanderRadius;
            _wanderRate = wanderRate;
            _wanderOrientation = startOrientation;
        }
    }


    public class Separation : BaseSteering
    {
        private const float DECAY = 3f;

        private StaticMovementData Character { get; init; }
        public ICollection<StaticMovementData> Targets { private get; set; }
        private float _sqrMinSep = 1f;

        public float MinSeparation
        {
            set
            {
                _sqrMinSep = value > 0 ? value * value : throw new ArgumentException("Separation must be positive");
            }
        }

        public override SteeringOutput GetSteering()
        {
            if (Targets.Count == 0)
                return SteeringOutput.NoSteering;

            Vector2 linear = Vector2.zero;
            foreach (StaticMovementData t in Targets)
            {
                Vector2 dir = t.Position - Character.Position;
                float sqrDist = dir.sqrMagnitude;
                if (sqrDist > _sqrMinSep)
                    continue;

                float strength = Mathf.Min(DECAY / sqrDist, MoveVars.MaxAcceleration);
                linear += -dir.normalized * strength;
            }

            return new SteeringOutput(linear, 0f);
        }

        public Separation(StaticMovementData character, AgentMovementVars moveVars, float separation) : base(moveVars)
        {
            MinSeparation = separation;
            Character = character ?? throw new ArgumentNullException(nameof(character));
        }
    }


    public class AgentsAvoidance : BaseSteering
    {
        private KinematicMovementData Character { get; init; }
        public ICollection<KinematicMovementData> Targets { private get; set; }

        public override SteeringOutput GetSteering()
        {
            if (Targets.Count == 0)
                return SteeringOutput.NoSteering;


            KinematicMovementData closestAgent = null;
            float closestDist = Mathf.Infinity;
            float closestTime = Mathf.Infinity;
            float closestMinSep = Mathf.Infinity;
            Vector2 closestRelPos = Vector2.zero;
            Vector2 closestRelVel = Vector2.zero;
            Vector2 relPos;

            foreach (KinematicMovementData t in Targets)
            {
                relPos = t.Position - Character.Position;
                Vector2 relVel = -(t.Velocity - Character.Velocity);
                float relSpeed = relVel.magnitude;
                float colTime = Vector2.Dot(relPos, relVel) / (relSpeed * relSpeed);
                float dist = relPos.magnitude;
                float minSep = dist - relSpeed * colTime;

                if (minSep > 2 * MoveVars.Radius)
                    continue;

                if (colTime <= 0 || colTime >= closestTime)
                    continue;

                closestAgent = t;
                closestDist = dist;
                closestTime = colTime;
                closestMinSep = minSep;
                closestRelPos = relPos;
                closestRelVel = relVel;
            }

            if (closestAgent is null)
                return SteeringOutput.NoSteering;

#if UNITY_EDITOR
            Debug.DrawCross(Character.Position + Character.Velocity * closestTime, 0.1f, Color.cyan);
#endif

            if (closestMinSep <= 0 || closestDist <= 2 * MoveVars.Radius)
                relPos = Character.Position - closestAgent.Position;
            else
                relPos = closestRelPos + closestRelVel * closestTime;

            return new SteeringOutput(relPos.normalized * MoveVars.MaxAcceleration, 0f);
        }

        public AgentsAvoidance(KinematicMovementData character, AgentMovementVars moveVars) : base(moveVars)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
        }
    }


    public class ObstacleAvoidance : Seek
    {
        private readonly IEnumerable<AvoidanceRay> _rays;
        private readonly LayerMask _avoidanceMask;
        public override SteeringOutput GetSteering()
        {
            if (Character.Velocity.sqrMagnitude == 0f)
                return SteeringOutput.NoSteering;

            Vector2? target = null;
            foreach (AvoidanceRay r in _rays)
            {
                RaycastHit2D hit = Physics2D.Raycast(r.Position, r.Direction, r.Lenght, _avoidanceMask);
                if (!hit)
                    continue;

                if (target is null)
                    target = hit.point;

                target += hit.normal * MoveVars.AvoidanceDistance;
#if UNITY_EDITOR
                Debug.DrawRay(hit.point, hit.normal * MoveVars.AvoidanceDistance);
                Debug.DrawCross(target.Value, 0.1f, Color.black);
#endif
                break; // Test what happens if all collisions get summed up
            }

            if (target is null)
                return SteeringOutput.NoSteering;

            Target.Position = target.Value;
            return base.GetSteering();
        }

        public ObstacleAvoidance(KinematicMovementData character,
                                 IEnumerable<AvoidanceRay> rays,
                                 LayerMask avoidanceMask,
                                 AgentMovementVars moveVars) : base(character, new(), moveVars)
        {
            _rays = rays;
            _avoidanceMask = avoidanceMask;
        }
    }
}