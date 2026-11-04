using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine;

public class EscapeSessionHost : Photon.PunBehaviour, IGalaxySession
{
    public GalaxySim Sim;

    public bool ShowDebug;

    GalaxySnapshot snapshot = new GalaxySnapshot();

    string lastPhase = string.Empty;

    string lastPoi = string.Empty;

    string lastRevealed = string.Empty;

    string lastEquipment = string.Empty;

    public GalaxySnapshot Snapshot
    {
        get { return snapshot; }
    }

    public void Awake()
    {
        if (Sim == null)
        {
            Sim = GalaxySim.CreateSample();
        }

        snapshot = Sim.Capture();
    }

    public void Update()
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        var power = ShipManager.instance != null ? ShipManager.instance.PowerLevel : 1f;
        Sim.Tick(Time.deltaTime, power);
        snapshot = Sim.Capture();
        WriteDiscreteProperties();
    }

    public void SubmitPilotCommand(float throttle, float turn)
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.isMasterClient)
        {
            Sim.SetCommand(new PilotCommand(throttle, turn));
            return;
        }

        if (photonView != null)
        {
            photonView.RPC("SubmitPilotCommandRpc", PhotonNetworkSettings.EventTarget, throttle, turn);
        }
    }

    public bool TryReveal(float arcDegrees, float range)
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.isMasterClient)
        {
            return Sim.TryReveal(arcDegrees, range);
        }

        if (photonView != null)
        {
            photonView.RPC("TryRevealRpc", PhotonNetworkSettings.EventTarget, arcDegrees, range);
        }

        return false;
    }

    public void DamageContact(string id, float amount)
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.isMasterClient)
        {
            Sim.DamageContact(id, amount);
            return;
        }

        if (photonView != null)
        {
            photonView.RPC("DamageContactRpc", PhotonNetworkSettings.EventTarget, id, amount);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(Sim.Pose.X);
            stream.SendNext(Sim.Pose.Z);
            stream.SendNext(Sim.Pose.HeadingDegrees);
            stream.SendNext(Sim.Pose.Speed);
            stream.SendNext(Sim.Storm.Radius);
            stream.SendNext(Sim.Hull);
            return;
        }

        Sim.Pose.X = (float)stream.ReceiveNext();
        Sim.Pose.Z = (float)stream.ReceiveNext();
        Sim.Pose.HeadingDegrees = (float)stream.ReceiveNext();
        Sim.Pose.Speed = (float)stream.ReceiveNext();
        Sim.Storm.Radius = (float)stream.ReceiveNext();
        Sim.Hull = (float)stream.ReceiveNext();
        snapshot = Sim.Capture();
    }

    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        if (PhotonNetwork.isMasterClient || propertiesThatChanged == null)
        {
            return;
        }

        ApplyDiscreteProperties(propertiesThatChanged);
        snapshot = Sim.Capture();
    }

    [PunRPC]
    void SubmitPilotCommandRpc(float throttle, float turn)
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        Sim.SetCommand(new PilotCommand(throttle, turn));
    }

    [PunRPC]
    void TryRevealRpc(float arcDegrees, float range)
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        Sim.TryReveal(arcDegrees, range);
    }

    [PunRPC]
    void DamageContactRpc(string id, float amount)
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        Sim.DamageContact(id, amount);
    }

    void WriteDiscreteProperties()
    {
        if (!PhotonNetwork.inRoom || !PhotonNetwork.isMasterClient || PhotonNetwork.room == null)
        {
            return;
        }

        var phase = Sim.Phase.Current.ToString();
        var poi = Sim.CurrentPoiId ?? string.Empty;
        var revealed = Sim.RevealedIds();
        var equipment = string.Join(",", Sim.Equipment.ToArray());
        if (phase == lastPhase && poi == lastPoi && revealed == lastRevealed && equipment == lastEquipment)
        {
            return;
        }

        lastPhase = phase;
        lastPoi = poi;
        lastRevealed = revealed;
        lastEquipment = equipment;

        var props = new Hashtable();
        props[EscapeSessionKeys.EscapePhase] = phase;
        props[EscapeSessionKeys.ActivePoiId] = poi;
        props[EscapeSessionKeys.RevealedContactIds] = revealed;
        props[EscapeSessionKeys.EquipmentIds] = equipment;
        props[EscapeSessionKeys.Hull] = Sim.Hull;
        PhotonNetwork.room.SetCustomProperties(props);
    }

    void ApplyDiscreteProperties(Hashtable props)
    {
        if (props.ContainsKey(EscapeSessionKeys.EscapePhase))
        {
            Sim.Phase.Current = ParsePhase((string)props[EscapeSessionKeys.EscapePhase]);
        }

        if (props.ContainsKey(EscapeSessionKeys.ActivePoiId))
        {
            Sim.CurrentPoiId = (string)props[EscapeSessionKeys.ActivePoiId] ?? string.Empty;
        }

        if (props.ContainsKey(EscapeSessionKeys.RevealedContactIds))
        {
            ApplyRevealed((string)props[EscapeSessionKeys.RevealedContactIds]);
        }

        if (props.ContainsKey(EscapeSessionKeys.EquipmentIds))
        {
            ApplyEquipment((string)props[EscapeSessionKeys.EquipmentIds]);
        }
    }

    void ApplyRevealed(string ids)
    {
        for (int i = 0; i < Sim.Map.Contacts.Count; i++)
        {
            Sim.Map.Contacts[i].Revealed = false;
        }

        if (string.IsNullOrEmpty(ids))
        {
            return;
        }

        var parts = ids.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            var contact = Sim.Map.FindContact(parts[i]);
            if (contact != null)
            {
                contact.Revealed = true;
            }
        }
    }

    void ApplyEquipment(string ids)
    {
        Sim.Equipment.Clear();
        if (string.IsNullOrEmpty(ids))
        {
            return;
        }

        var parts = ids.Split(',');
        for (int i = 0; i < parts.Length; i++)
        {
            Sim.SetEquipment(parts[i]);
        }
    }

    static EscapePhase ParsePhase(string value)
    {
        if (value == EscapePhase.Transiting.ToString())
        {
            return EscapePhase.Transiting;
        }

        if (value == EscapePhase.MissionActive.ToString())
        {
            return EscapePhase.MissionActive;
        }

        if (value == EscapePhase.Escaped.ToString())
        {
            return EscapePhase.Escaped;
        }

        if (value == EscapePhase.Consumed.ToString())
        {
            return EscapePhase.Consumed;
        }

        return EscapePhase.Docked;
    }

    void OnGUI()
    {
        if (!ShowDebug || Sim == null)
        {
            return;
        }

        GUI.Label(new Rect(8f, 8f, 640f, 24f), string.Format("{0} hull {1:0} storm {2:0} poi {3}", Sim.Phase.Current, Sim.Hull, Sim.Storm.Radius, Sim.CurrentPoiId));
    }
}
