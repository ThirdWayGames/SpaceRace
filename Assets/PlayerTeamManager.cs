using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTeamManager : Photon.MonoBehaviour
{
    public void Update()
    {
        // All the blue players
        var bluePlayerInfos = GameObject.FindGameObjectsWithTag("BluePlayerInfo").Reverse().ToList();
        bluePlayerInfos.ForEach(x => x.SetActive(false));

        var bluePlayers = PhotonNetwork.playerList.Where(x => (int)x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] == 1).Select((x, i) => new { Player = x, Index = i }).ToList();
        foreach (var bluePlayer in bluePlayers)
        {
            bluePlayerInfos[bluePlayer.Index].GetComponentInChildren<Text>().text = bluePlayer.Player.NickName;
            bluePlayerInfos[bluePlayer.Index].GetComponentInChildren<Button>().tag = bluePlayer.Player.UserId;
            bluePlayerInfos[bluePlayer.Index].GetComponentInChildren<Image>().tag = bluePlayer.Player.UserId;
            bluePlayerInfos[bluePlayer.Index].SetActive(true);
        }

        // All the red players.
        var redPlayerInfos = GameObject.FindGameObjectsWithTag("RedPlayerInfo").Reverse().ToList();
        redPlayerInfos.ForEach(x => x.SetActive(false));

        var redPlayers = PhotonNetwork.playerList.Where(x => (int)x.CustomProperties[PlayerManager3D.PlayerTeamPrefKey] == 2).Select((x, i) => new { Player = x, Index = i }).ToList();
        foreach (var redPlayer in redPlayers)
        {
            redPlayerInfos[redPlayer.Index].GetComponentInChildren<Text>().text = redPlayer.Player.NickName;
            redPlayerInfos[redPlayer.Index].SetActive(true);
        }
    }

    public void PlayerReady(string playerIndex)
    {
    }
}
