using UnityEngine;

public static class GalaxySessionFinder
{
    public static IGalaxySession Find()
    {
        var behaviours = Object.FindObjectsOfType<MonoBehaviour>();
        for (int i = 0; i < behaviours.Length; i++)
        {
            var session = behaviours[i] as IGalaxySession;
            if (session != null)
            {
                return session;
            }
        }

        return null;
    }
}
