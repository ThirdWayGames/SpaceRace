using Steamworks;
using UnityEngine;

public class SteamAchievements : MonoBehaviour
{
    public void Awake()
    {
        this.gameObject.SetActive(SteamManager.Initialized);
    }

    public void LockSteamAchievement(string id)
    {
        if (TestSteamAchievement(id))
        {
            SteamUserStats.ClearAchievement(id);
        }
    }

    public void UnlockSteamAchievement(string id)
    {
        if (!TestSteamAchievement(id))
        {
            SteamUserStats.SetAchievement(id);
            SteamUserStats.StoreStats();
        }
    }

    public bool TestSteamAchievement(string id)
    {
        bool achievementComplete = false;
        SteamUserStats.GetAchievement(id, out achievementComplete);
        return achievementComplete;
    }
}
