using System;

using ExitGames.Client.Photon;

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Dropdown))]
public class PlayerTeamField : MonoBehaviour 
{
    private Hashtable PlayerCustomProperties = new Hashtable();

    public void Start()
    {
        SetDefaultProps();
    }

    public void SetDefaultProps()
    {
        int defaultTeam = 0;
        Dropdown _dropDown = this.GetComponent<Dropdown>();
        if (_dropDown != null)
        {
            if (PlayerPrefs.HasKey(PlayerManager3D.PlayerTeamPrefKey))
            {
                defaultTeam = PlayerPrefs.GetInt(PlayerManager3D.PlayerTeamPrefKey);
                _dropDown.value = defaultTeam;
                if (!PlayerCustomProperties.ContainsKey(PlayerManager3D.PlayerTeamPrefKey))
                {
                    this.PlayerCustomProperties.Add(PlayerManager3D.PlayerTeamPrefKey, defaultTeam);
                }
                else
                {
                    this.PlayerCustomProperties[PlayerManager3D.PlayerTeamPrefKey] = defaultTeam;
                }
            }
        }

        PhotonNetwork.player.SetCustomProperties(this.PlayerCustomProperties);
    }

    public void SetPlayerTeamId(int value)
    {
        // Set the player prefs.
        PlayerPrefs.SetInt(PlayerManager3D.PlayerTeamPrefKey, value);

        // Update the local custom props
        this.PlayerCustomProperties[PlayerManager3D.PlayerTeamPrefKey] = value;

        // Update the player custom props.
        PhotonNetwork.player.SetCustomProperties(this.PlayerCustomProperties);
    }
}
