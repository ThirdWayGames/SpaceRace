using System.Collections.Generic;

public class GalaxyMap
{
    public List<PointOfInterest> Points = new List<PointOfInterest>();

    public List<GalaxyContact> Contacts = new List<GalaxyContact>();

    public static GalaxyMap CreateSample()
    {
        var map = new GalaxyMap();
        map.Points.Add(new PointOfInterest { Id = "wreck", X = 0f, Z = 200f, Radius = 25f, MissionId = "greybox", EquipmentRewardId = "coil" });
        map.Points.Add(new PointOfInterest { Id = "planet", X = 80f, Z = 80f, Radius = 25f, MissionId = "greybox" });
        map.Points.Add(new PointOfInterest { Id = "hostile-ship", X = -80f, Z = 120f, Radius = 25f, MissionId = "greybox" });
        map.Points.Add(new PointOfInterest { Id = "eye", X = 0f, Z = 0f, Radius = 20f, IsWormhole = true });
        map.Contacts.Add(new GalaxyContact { Id = "radiation", X = 40f, Z = 160f, Radius = 18f, Kind = GalaxyContactKind.Hazard });
        map.Contacts.Add(new GalaxyContact { Id = "raider", X = -40f, Z = 220f, Radius = 16f, Kind = GalaxyContactKind.Hostile, Health = 30f });
        return map;
    }

    public PointOfInterest FindPoint(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        for (int i = 0; i < Points.Count; i++)
        {
            if (Points[i].Id == id)
            {
                return Points[i];
            }
        }

        return null;
    }

    public GalaxyContact FindContact(string id)
    {
        for (int i = 0; i < Contacts.Count; i++)
        {
            if (Contacts[i].Id == id)
            {
                return Contacts[i];
            }
        }

        return null;
    }
}
