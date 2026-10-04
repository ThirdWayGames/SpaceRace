using UnityEngine;

public class TestPlayerSpawner : MonoBehaviour
{
    public void ForcePlayerSpawn(int teamIdOverride = -1)
    {
        if (teamIdOverride >= 0 && GameManager3D.instance != null)
        {
            GameManager3D.instance.PlayerTeamOverride = teamIdOverride;
        }

        var playerManager = PlayerManager3D.Get();
        if (playerManager != null)
        {
            playerManager.SpawnPlayer();
        }
    }
}
