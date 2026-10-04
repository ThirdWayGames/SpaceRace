using Steamworks;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InputField))]
public class PlayerNameInputField : MonoBehaviour
{
    private static string playerNamePrefKey = "PlayerName";
    
    void Start ()
    {
        string defaultName = SteamManager.Initialized ? SteamFriends.GetPersonaName() : string.Empty;
        InputField _inputField = this.GetComponent<InputField>();
        if (_inputField != null)
        {
            if (!SteamManager.Initialized)
            {
                if (PlayerPrefs.HasKey(playerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(playerNamePrefKey);
                }
            }
            else
            {
                _inputField.enabled = false;
            }

            _inputField.text = defaultName;
        }

        PhotonNetwork.playerName = defaultName;
    }

    public void SetPlayerName(string value)
    {
        PhotonNetwork.playerName = value + " ";
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
