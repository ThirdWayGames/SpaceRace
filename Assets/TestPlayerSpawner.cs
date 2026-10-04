using Assets.Scripts.Interfaces;
using UnityEngine;

public class TestPlayerSpawner : MonoBehaviour
{
    public void ForcePlayerSpawn(int teamIdOverride = -1)
    {
        GameManager3D.ConsoleMsg("Spawning Player");
        GameManager3D.ConsoleMsg("No Local Player Instance Found");
        var spawnLoc = GameManager3D.GetTeleportForCurrentPlayer();
        if (spawnLoc == null)
        {
            GameManager3D.ConsoleMsg("Spawn Loc was null");
            return;
        }

        int? tmpTeamId = null;
        if (teamIdOverride >= 0) tmpTeamId = teamIdOverride;
        GameManager3D.ConsoleMsg(string.Format("{0} Spawning", PhotonNetwork.inRoom ? "Network" : "Local"));
        var marinePrefab = GameManager3D.instance.GetMarinePrefab();
        var player = PhotonNetwork.inRoom ? PhotonNetwork.Instantiate(marinePrefab.name, spawnLoc.GetSpawnPos(), spawnLoc.GetSpawnRot(), 0) : Instantiate(marinePrefab, spawnLoc.GetSpawnPos(), spawnLoc.GetSpawnRot());
        player.layer = LayerMask.NameToLayer("Player");

        // Get the player controller.
        if (player != null)
        {
            var playerAnimController = player.GetComponentInChildren<PlayerAnimController>();
            if (playerAnimController != null)
            {
                playerAnimController.SetTeam(PhotonNetwork.inRoom ? (int)PhotonNetwork.player.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] : 0);
            }

            var playerController = player.GetComponent<IPlayerController>();
            if (playerController != null)
            {
                // Set the spawn loc as used.
                spawnLoc.Spawned(playerController);
            }
        }
    }
}
