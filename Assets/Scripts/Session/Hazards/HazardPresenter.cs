using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

/// <summary>
/// Applies one contact after a sim tick. The session host calls Apply; this component does not tick itself.
/// Pass the contact again with Overlapping false when the ship leaves it so the visit and alarm can end.
/// </summary>
public class HazardPresenter : MonoBehaviour
{
    public const float BoarderOffsetMetres = 2f;

    public string BoarderPrefabName = string.Empty;

    public Transform BoarderSpawn;

    readonly BoarderVisits boarderVisits = new BoarderVisits();

    readonly HashSet<string> pathogenVisitIds = new HashSet<string>();

    readonly HashSet<string> alarmingContactIds = new HashSet<string>();

    bool alarmHeld;

    bool loggedEmptyBoarderPrefab;

    public void Apply(ContactState contact)
    {
        if (contact == null || !IsSessionAuthority())
        {
            return;
        }

        var effect = ContactEffectTable.Resolve(contact);
        ApplyAlarm(contact, effect);
        ApplyBoarder(contact, effect);
        ApplyPathogen(contact, effect);
    }

    public Vector3 BoarderSpawnPosition()
    {
        Transform spawn = BoarderSpawn;
        if (spawn == null)
        {
            spawn = FindGreyboxSpawn();
        }

        if (spawn != null)
        {
            return spawn.position + (spawn.right * BoarderOffsetMetres);
        }

        return transform.position + (Vector3.right * BoarderOffsetMetres);
    }

    static bool IsSessionAuthority()
    {
        if (!PhotonNetwork.inRoom)
        {
            return true;
        }

        return PhotonNetwork.isMasterClient;
    }

    void ApplyAlarm(ContactState contact, ContactEffect effect)
    {
        if (string.IsNullOrEmpty(contact.Id))
        {
            return;
        }

        if (effect.RaiseAlarm)
        {
            alarmingContactIds.Add(contact.Id);
        }
        else
        {
            alarmingContactIds.Remove(contact.Id);
        }

        bool wantAlarm = alarmingContactIds.Count > 0;
        if (wantAlarm == alarmHeld)
        {
            return;
        }

        if (ShipManager.instance == null)
        {
            return;
        }

        alarmHeld = wantAlarm;
        GameManager3D.SetAlarmActive(wantAlarm);
    }

    void ApplyBoarder(ContactState contact, ContactEffect effect)
    {
        if (!contact.Overlapping)
        {
            boarderVisits.EndVisit(contact.Id);
            return;
        }

        if (!effect.SpawnBoarder)
        {
            return;
        }

        if (!boarderVisits.TryBeginVisit(contact.Id))
        {
            return;
        }

        TrySpawnBoarder();
    }

    void TrySpawnBoarder()
    {
        if (string.IsNullOrWhiteSpace(BoarderPrefabName))
        {
            if (!loggedEmptyBoarderPrefab)
            {
                loggedEmptyBoarderPrefab = true;
                Debug.Log("HazardPresenter: BoarderPrefabName is empty, skipping boarder instantiate.");
            }

            return;
        }

        if (!PhotonNetwork.inRoom)
        {
            return;
        }

        PhotonNetwork.Instantiate(BoarderPrefabName, BoarderSpawnPosition(), Quaternion.identity, 0);
    }

    void ApplyPathogen(ContactState contact, ContactEffect effect)
    {
        if (!contact.Overlapping)
        {
            if (!string.IsNullOrEmpty(contact.Id))
            {
                pathogenVisitIds.Remove(contact.Id);
            }

            return;
        }

        if (string.IsNullOrEmpty(effect.PathogenResourceName) || string.IsNullOrEmpty(contact.Id))
        {
            return;
        }

        if (pathogenVisitIds.Contains(contact.Id))
        {
            return;
        }

        pathogenVisitIds.Add(contact.Id);
        InfectAboardPlayers(effect.PathogenResourceName);
    }

    static void InfectAboardPlayers(string pathogenName)
    {
        var players = Object.FindObjectsOfType<BasePlayerController3D>();
        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            if (player == null)
            {
                continue;
            }

            var view = player.GetComponent<PhotonView>();
            if (!IsAboard(view))
            {
                continue;
            }

            var mutation = player.GetComponent<MutationController>();
            if (mutation == null)
            {
                continue;
            }

            int teamId = player.GetTeamId();
            if (PhotonNetwork.inRoom)
            {
                if (view == null)
                {
                    continue;
                }

                view.RPC("AddPathogen", PhotonNetworkSettings.EventTarget, pathogenName, teamId);
            }
            else
            {
                mutation.AddPathogen(pathogenName, teamId);
            }
        }
    }

    static bool IsAboard(PhotonView view)
    {
        if (!PhotonNetwork.inRoom || view == null || view.owner == null)
        {
            return true;
        }

        var props = view.owner.CustomProperties;
        if (props == null || !props.ContainsKey(EscapeSessionKeys.CrewLocation))
        {
            return true;
        }

        return IsAboardValue(props[EscapeSessionKeys.CrewLocation]);
    }

    static bool IsAboardValue(object raw)
    {
        if (raw is int)
        {
            return (int)raw == (int)CrewLocation.Aboard;
        }

        if (raw is CrewLocation)
        {
            return (CrewLocation)raw == CrewLocation.Aboard;
        }

        if (raw is string)
        {
            return (string)raw != CrewLocation.Deployed.ToString();
        }

        return true;
    }

    static Transform FindGreyboxSpawn()
    {
        var greybox = GameObject.Find("ShipGreybox");
        if (greybox == null)
        {
            return null;
        }

        var children = greybox.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (IsSpawnMarker(children[i].name))
            {
                return children[i];
            }
        }

        return greybox.transform;
    }

    static bool IsSpawnMarker(string name)
    {
        return name == "Spawn"
            || name == "PlayerSpawn"
            || name == "ShipSpawn"
            || name == "SpawnMarker"
            || name == "SpawnPad";
    }
}
