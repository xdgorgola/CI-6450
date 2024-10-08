using System;
using System.Collections.Generic;
using UnityEngine;

namespace IA.Steering
{
    public interface ISteering
    {
        public SteeringOutput GetSteering();
    }

    public struct SteeringOutput
    {
        public readonly Vector2 linear;
        public readonly float angular;

        public static SteeringOutput NoSteering =>
            new SteeringOutput(Vector2.zero, 0f);

        public SteeringOutput(Vector2 linear, float angular)
        {
            this.linear = linear;
            this.angular = angular;
        }
    }

    public struct KinematicSteeringOutput
    {
        public readonly Vector2 velocity;
        public readonly float rotation;

        public KinematicSteeringOutput(Vector2 velocity, float rotation)
        {
            this.velocity = velocity;
            this.rotation = rotation;
        }
    }

    public abstract class BaseSteering : ISteering
    {
        protected AgentMovementVars MoveVars { get; init; }
        public virtual SteeringOutput GetSteering() { throw new System.NotImplementedException(); }

        public BaseSteering(AgentMovementVars moveVars)
        {
            MoveVars = moveVars ?? throw new ArgumentNullException(nameof(moveVars)); ;
        }
    }


    [System.Serializable]
    public struct AvoidanceRay 
    {
        [SerializeField]
        private Transform _origin;
        [SerializeField]
        [Min(0.5f)]
        private float _lenght;

        public Vector2 Position { get { return _origin.position; } }
        public Vector2 Direction { get { return _origin.right; } }
        public Vector2 Ray { get { return _origin.right * _lenght; } }
        public float Lenght { get { return _lenght; } }

        public AvoidanceRay(Transform origin, float lenght)
        {
            this._origin = origin ?? throw new ArgumentNullException(nameof(origin));
            this._lenght = lenght;
        }
    }

    [System.Serializable]
    public struct AgentPatrolPath
    {
        [SerializeField]
        private List<Transform> _points;

        public int ClosestControlPoint(Vector2 pos)
        {
            int cSeg = -1;
            float cDist = float.PositiveInfinity;

            for (int i = 0; i < _points.Count; ++i)
            {
                Vector2 pp = (Vector2)_points[i].position;
                float sqrDist = (pp - pos).sqrMagnitude;
                if (sqrDist >= cDist)
                    continue;

                cSeg = i;
                cDist = sqrDist;
            }

            return cSeg;
        }

        public Vector2 ClosestPointInPath(Vector2 pos, int curControl)
        {
            return Vector2.zero;
        }
    }
}