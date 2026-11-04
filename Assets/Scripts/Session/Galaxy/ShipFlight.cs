using UnityEngine;

public static class ShipFlight
{
    public const float MaxSpeed = 12f;

    public const float Acceleration = 8f;

    public const float TurnRate = 45f;

    public static ShipPose Integrate(ShipPose pose, PilotCommand command, float powerLevel, float dt)
    {
        var result = pose == null ? new ShipPose() : pose.ClonePose();
        var order = command == null ? new PilotCommand() : command;
        order.Clamp();
        var power = Mathf.Clamp01(powerLevel);
        if (dt < 0f)
        {
            dt = 0f;
        }

        result.Speed += Acceleration * power * order.Throttle * dt;
        result.Speed = Mathf.Clamp(result.Speed, 0f, MaxSpeed);
        result.HeadingDegrees += TurnRate * power * order.Turn * dt;

        var radians = result.HeadingDegrees * Mathf.Deg2Rad;
        result.X += Mathf.Sin(radians) * result.Speed * dt;
        result.Z += Mathf.Cos(radians) * result.Speed * dt;
        return result;
    }
}
