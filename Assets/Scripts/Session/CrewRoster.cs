using System.Collections.Generic;

public class CrewRoster
{
    readonly Dictionary<int, CrewLocation> locations = new Dictionary<int, CrewLocation>();

    public void Set(int playerId, CrewLocation location)
    {
        locations[playerId] = location;
    }

    public void Remove(int playerId)
    {
        if (locations.ContainsKey(playerId))
        {
            locations.Remove(playerId);
        }
    }

    public CrewLocation LocationOf(int playerId)
    {
        CrewLocation location;
        if (locations.TryGetValue(playerId, out location))
        {
            return location;
        }

        return CrewLocation.Aboard;
    }

    /// <summary>
    /// True when every listed player is aboard. An empty roster is aboard so a new session can undock.
    /// Dead players are not stored here. Callers set them aboard or remove them.
    /// </summary>
    public bool AllAboard()
    {
        foreach (var pair in locations)
        {
            if (pair.Value != CrewLocation.Aboard)
            {
                return false;
            }
        }

        return true;
    }

    public bool AnyDeployed()
    {
        return DeployedCount() > 0;
    }

    public int DeployedCount()
    {
        var count = 0;
        foreach (var pair in locations)
        {
            if (pair.Value == CrewLocation.Deployed)
            {
                count += 1;
            }
        }

        return count;
    }
}
