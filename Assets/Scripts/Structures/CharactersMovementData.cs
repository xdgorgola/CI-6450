using UnityEngine;

namespace IA.Steering
{
    public class StaticMovementData
    {
        public Vector2 Position { get; set; }
        public float Orientation { get; set; }

        public StaticMovementData(Vector2 position, float orientation)
        {
            Position = position;
            Orientation = orientation;
        }

        public StaticMovementData()
        {
            Position = Vector2.zero;
            Orientation = 0f;
        }
    }


    public class KinematicMovementData : StaticMovementData
    {
        public Vector2 Velocity { get; set; }
        public float Rotation { get; set; }

        public KinematicMovementData(Vector2 position, float orientation, Vector2 velocity, float rotation) : base(position, orientation)
        {
            Velocity = velocity;
            Rotation = rotation;
        }

        public KinematicMovementData(): base(Vector2.zero, 0f)
        {
            Velocity = Vector2.zero;
            Rotation = 0f;
        }
    }
}