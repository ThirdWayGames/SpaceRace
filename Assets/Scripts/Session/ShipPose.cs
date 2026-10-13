using System;

[Serializable]
public class ShipPose
{
    public float X;

    public float Z;

    public float HeadingDegrees;

    public float Speed;

    public ShipPose()
    {
    }

    public ShipPose(float x, float z, float headingDegrees, float speed)
    {
        X = x;
        Z = z;
        HeadingDegrees = headingDegrees;
        Speed = speed;
    }

    public ShipPose ClonePose()
    {
        return new ShipPose(X, Z, HeadingDegrees, Speed);
    }
}
