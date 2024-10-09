using UnityEngine;

public static class MathUtils
{
    public const float STOP_EPSILON = 0.001f;

    public static Vector2 VectorFromOrientation(float orientation) =>
    new Vector2(Mathf.Cos(orientation), Mathf.Sin(orientation));

    public static float RandomBinomial() =>
        Random.value - Random.value;


    public static float NewOrientation(float cur, Vector2 vel)
    {
        if (vel.sqrMagnitude > 0)
            return Mathf.Atan2(vel.y, vel.x);

        return cur;
    }


    public static (float radOr, float degOr) WrapOrientation(float cur)
    {
        float orientationDegs = cur * Mathf.Rad2Deg;
        if (orientationDegs > 360f)
            orientationDegs = orientationDegs - 360f;
        else if (orientationDegs < 0)
            orientationDegs = 360f + orientationDegs;

        return (orientationDegs * Mathf.Deg2Rad, orientationDegs);
    }


    public static Vector2 RandomDirection() =>
        Random.insideUnitCircle.normalized;

    public static Vector3 RadRotationToUnity(float rad)
    {
        float ang = Mathf.Abs(Mathf.Rad2Deg * rad);
        if (rad > 0)
            return new(0f, 0f, ang);
        return new(0f, 0f, 360f - ang);
    }
}
