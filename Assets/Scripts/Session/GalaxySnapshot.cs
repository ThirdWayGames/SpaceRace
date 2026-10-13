using System;

[Serializable]
public class GalaxySnapshot
{
    public float X;

    public float Z;

    public float HeadingDegrees;

    public float Speed;

    public float StormRadius;

    public float Hull;

    public string PhaseName = string.Empty;

    public string CurrentPoiId = string.Empty;

    public string RevealedContactIds = string.Empty;

    public string ContactSummary = string.Empty;
}
