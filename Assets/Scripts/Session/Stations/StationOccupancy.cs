using System.Collections.Generic;

public class StationOccupancy
{
    readonly Dictionary<string, int> seats = new Dictionary<string, int>();

    public bool TrySit(string stationId, int playerId)
    {
        if (string.IsNullOrEmpty(stationId) || playerId == 0)
        {
            return false;
        }

        var current = StationFor(playerId);
        if (current == stationId)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(current))
        {
            return false;
        }

        if (seats.ContainsKey(stationId))
        {
            return false;
        }

        seats[stationId] = playerId;
        return true;
    }

    public bool TryStand(int playerId)
    {
        var stationId = StationFor(playerId);
        if (string.IsNullOrEmpty(stationId))
        {
            return false;
        }

        seats.Remove(stationId);
        return true;
    }

    public string StationFor(int playerId)
    {
        foreach (var pair in seats)
        {
            if (pair.Value == playerId)
            {
                return pair.Key;
            }
        }

        return string.Empty;
    }

    public int PlayerIn(string stationId)
    {
        int playerId;
        if (seats.TryGetValue(stationId, out playerId))
        {
            return playerId;
        }

        return 0;
    }

    public string Write()
    {
        var parts = new List<string>();
        foreach (var pair in seats)
        {
            parts.Add(pair.Key + ":" + pair.Value);
        }

        return string.Join(",", parts.ToArray());
    }

    public void Read(string encoded)
    {
        seats.Clear();
        if (string.IsNullOrEmpty(encoded))
        {
            return;
        }

        var parts = encoded.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            var piece = parts[i];
            var split = piece.Split(':');
            if (split.Length != 2)
            {
                continue;
            }

            int playerId;
            if (!int.TryParse(split[1], out playerId))
            {
                continue;
            }

            if (!seats.ContainsKey(split[0]))
            {
                seats[split[0]] = playerId;
            }
        }
    }
}
