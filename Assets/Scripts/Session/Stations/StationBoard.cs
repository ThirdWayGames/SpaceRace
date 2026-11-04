using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

public class StationBoard : Photon.PunBehaviour
{
    public StationOccupancy Occupancy = new StationOccupancy();

    string lastWritten = string.Empty;

    public static int LocalActorId()
    {
        if (PhotonNetwork.inRoom && PhotonNetwork.player != null)
        {
            return PhotonNetwork.player.ID;
        }

        return 1;
    }

    public static StationBoard FindBoard()
    {
        return FindObjectOfType<StationBoard>();
    }

    public void Start()
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null || PhotonNetwork.room.CustomProperties == null)
        {
            return;
        }

        if (PhotonNetwork.room.CustomProperties.ContainsKey(EscapeSessionKeys.StationSeats))
        {
            Occupancy.Read((string)PhotonNetwork.room.CustomProperties[EscapeSessionKeys.StationSeats]);
            lastWritten = Occupancy.Write();
        }
    }

    public void RequestSit(string stationId)
    {
        if (!PhotonNetwork.inRoom)
        {
            if (Occupancy.TrySit(stationId, LocalActorId()))
            {
                lastWritten = Occupancy.Write();
            }

            return;
        }

        if (photonView != null)
        {
            photonView.RPC("RequestSitRpc", PhotonNetworkSettings.EventTarget, stationId);
        }
    }

    public void RequestStand()
    {
        if (!PhotonNetwork.inRoom)
        {
            if (Occupancy.TryStand(LocalActorId()))
            {
                lastWritten = Occupancy.Write();
            }

            return;
        }

        if (photonView != null)
        {
            photonView.RPC("RequestStandRpc", PhotonNetworkSettings.EventTarget);
        }
    }

    public void RequestTurretFire()
    {
        if (!PhotonNetwork.inRoom)
        {
            FireTurret(LocalActorId());
            return;
        }

        if (photonView != null)
        {
            photonView.RPC("RequestTurretFireRpc", PhotonNetworkSettings.EventTarget);
        }
    }

    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged == null || !propertiesThatChanged.ContainsKey(EscapeSessionKeys.StationSeats))
        {
            return;
        }

        Occupancy.Read((string)propertiesThatChanged[EscapeSessionKeys.StationSeats]);
        lastWritten = Occupancy.Write();
    }

    [PunRPC]
    public void RequestSitRpc(string stationId, PhotonMessageInfo info)
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        var actorId = info.sender != null ? info.sender.ID : LocalActorId();
        if (!Occupancy.TrySit(stationId, actorId))
        {
            return;
        }

        Commit();
    }

    [PunRPC]
    public void RequestStandRpc(PhotonMessageInfo info)
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        var actorId = info.sender != null ? info.sender.ID : LocalActorId();
        if (!Occupancy.TryStand(actorId))
        {
            return;
        }

        Commit();
    }

    [PunRPC]
    public void RequestTurretFireRpc(PhotonMessageInfo info)
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        var actorId = info.sender != null ? info.sender.ID : LocalActorId();
        FireTurret(actorId);
    }

    [PunRPC]
    public void PlayTurretShot()
    {
    }

    void FireTurret(int actorId)
    {
        var stationId = Occupancy.StationFor(actorId);
        var station = CrewStation.Find(stationId);
        if (station == null || station.Role != StationRole.Turret)
        {
            return;
        }

        var session = GalaxySessionFinder.Find();
        if (session == null || session.Snapshot == null)
        {
            return;
        }

        var target = TurretAim.Pick(session.Snapshot.X, session.Snapshot.Z, session.Snapshot.HeadingDegrees, session.Snapshot.ContactSummary);
        if (string.IsNullOrEmpty(target))
        {
            return;
        }

        session.DamageContact(target, TurretAim.Damage);
        if (PhotonNetwork.inRoom && photonView != null)
        {
            photonView.RPC("PlayTurretShot", PhotonNetworkSettings.EventTarget);
        }
    }

    void Commit()
    {
        var encoded = Occupancy.Write();
        if (encoded == lastWritten)
        {
            return;
        }

        lastWritten = encoded;
        if (!PhotonNetwork.inRoom || !PhotonNetwork.isMasterClient || PhotonNetwork.room == null)
        {
            return;
        }

        var props = new Hashtable();
        props[EscapeSessionKeys.StationSeats] = encoded;
        PhotonNetwork.room.SetCustomProperties(props);
    }
}
