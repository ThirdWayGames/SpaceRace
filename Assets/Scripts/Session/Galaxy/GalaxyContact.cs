using System;

[Serializable]
public class GalaxyContact
{
    public string Id = string.Empty;

    public float X;

    public float Z;

    public float Radius;

    public GalaxyContactKind Kind;

    public bool Revealed;

    public float Health;
}
