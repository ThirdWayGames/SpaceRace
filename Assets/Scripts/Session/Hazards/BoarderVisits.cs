using System.Collections.Generic;

public class BoarderVisits
{
    readonly HashSet<string> activeIds = new HashSet<string>();

    public bool TryBeginVisit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        if (activeIds.Contains(id))
        {
            return false;
        }

        activeIds.Add(id);
        return true;
    }

    public void EndVisit(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return;
        }

        activeIds.Remove(id);
    }
}
