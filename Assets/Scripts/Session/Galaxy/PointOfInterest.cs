using System;

[Serializable]
public class PointOfInterest
{
    public string Id = string.Empty;

    public float X;

    public float Z;

    public float Radius;

    public string MissionId = string.Empty;

    public string EquipmentRewardId = string.Empty;

    public bool IsWormhole;
}
