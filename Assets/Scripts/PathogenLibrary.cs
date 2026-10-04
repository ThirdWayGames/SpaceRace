using Assets.Scripts.ScriptableObjects.Pathogens;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PathogenLibrary : Photon.PunBehaviour
{
    public const string KnownPathogensKey = "KnownPathogens";

    public const string PathogenCuresKey = "PathogenCures";

    public List<ScriptableObject> InitialisationList;

    public List<KeyValuePair<int, List<string>>> TeamPathogenCures;

    public List<KeyValuePair<int, List<string>>> TeamKnownPathogens;

    public void Awake()
    {
        TeamPathogenCures = new List<KeyValuePair<int, List<string>>>();
        TeamKnownPathogens = new List<KeyValuePair<int, List<string>>>();

        if (InitialisationList.Any())
        {
            var pathLibInits = InitialisationList.Cast<PathogenLibInitialiser>();
            if (pathLibInits.Any())
            {
                foreach (var pathLibInt in pathLibInits)
                {
                    foreach (var pathgen in pathLibInt.CurablePathogens)
                    {
                        ApplyPathogen(pathLibInt.TeamId, pathgen.name, true);
                    }

                    foreach (var pathgen in pathLibInt.KnownPathogens)
                    {
                        ApplyPathogen(pathLibInt.TeamId, pathgen.name);
                    }
                }
            }
        }
    }

    public void Start()
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null)
        {
            return;
        }

        if (PhotonNetwork.isMasterClient)
        {
            PublishMembership();
        }
        else
        {
            ReadMembership(PhotonNetwork.room.CustomProperties);
        }
    }

    public void AddPathogenLocally(int teamId, string pathogenName, bool isCure = false)
    {
        if (this.photonView != null && PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            this.photonView.RPC("AddPathogen", PhotonTargets.MasterClient, teamId, pathogenName, isCure);
            return;
        }

        this.AddPathogen(teamId, pathogenName, isCure);
    }

    [PunRPC]
    public void AddPathogen(int teamId, string pathogenName, bool isCure = false)
    {
        if (PhotonNetwork.inRoom && !PhotonNetwork.isMasterClient)
        {
            return;
        }

        ApplyPathogen(teamId, pathogenName, isCure);
        PublishMembership();
    }

    public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        ReadMembership(propertiesThatChanged);
    }

    public bool CanCurePathogen(int teamId, string pathogenName)
    {
        var result = false;
        if (TeamPathogenCures.Any(x => x.Key == teamId))
        {
            var teamCures = TeamPathogenCures.FirstOrDefault(x => x.Key == teamId);
            result = teamCures.Value.Contains(pathogenName);
        }

        return result;
    }

    public string GetCurrentTeamPathogen(int teamId)
    {
        string pathogenToGet = string.Empty;
        var pathogens = TeamKnownPathogens.FirstOrDefault(x => x.Key == teamId);
        if (pathogens.Value != null && pathogens.Value.Any())
        {
            pathogenToGet = pathogens.Value.Last();
        }

        return pathogenToGet;
    }

    void ApplyPathogen(int teamId, string pathogenName, bool isCure = false)
    {
        var pathogenList = isCure ? TeamPathogenCures : TeamKnownPathogens;

        if (!pathogenList.Any(x => x.Key == teamId))
        {
            pathogenList.Add(new KeyValuePair<int, List<string>>(teamId, new List<string>()));
        }

        var pathogens = pathogenList.FirstOrDefault(x => x.Key == teamId);

        if (!pathogens.Value.Contains(pathogenName))
        {
            pathogens.Value.Add(pathogenName);
            Debug.LogFormat("Adding {0} for team {1}", pathogenName, teamId);
        }
    }

    void PublishMembership()
    {
        if (!PhotonNetwork.inRoom || PhotonNetwork.room == null)
        {
            return;
        }

        var props = new Hashtable();
        props[KnownPathogensKey] = SerializeMembership(TeamKnownPathogens);
        props[PathogenCuresKey] = SerializeMembership(TeamPathogenCures);
        PhotonNetwork.room.SetCustomProperties(props);
    }

    void ReadMembership(Hashtable properties)
    {
        if (properties == null)
        {
            return;
        }

        if (properties.ContainsKey(KnownPathogensKey))
        {
            TeamKnownPathogens = DeserializeMembership((string)properties[KnownPathogensKey]);
        }

        if (properties.ContainsKey(PathogenCuresKey))
        {
            TeamPathogenCures = DeserializeMembership((string)properties[PathogenCuresKey]);
        }
    }

    static string SerializeMembership(List<KeyValuePair<int, List<string>>> membership)
    {
        var builder = new StringBuilder();
        foreach (var entry in membership)
        {
            if (builder.Length > 0)
            {
                builder.Append(';');
            }

            builder.Append(entry.Key);
            builder.Append('=');
            if (entry.Value != null)
            {
                builder.Append(string.Join(",", entry.Value.ToArray()));
            }
        }

        return builder.ToString();
    }

    static List<KeyValuePair<int, List<string>>> DeserializeMembership(string serialized)
    {
        var membership = new List<KeyValuePair<int, List<string>>>();
        if (string.IsNullOrEmpty(serialized))
        {
            return membership;
        }

        var entries = serialized.Split(';');
        foreach (var entry in entries)
        {
            if (string.IsNullOrEmpty(entry))
            {
                continue;
            }

            var parts = entry.Split(new[] { '=' }, 2);
            int teamId;
            if (parts.Length == 0 || !int.TryParse(parts[0], out teamId))
            {
                continue;
            }

            var names = new List<string>();
            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
            {
                var parsedNames = parts[1].Split(',');
                foreach (var name in parsedNames)
                {
                    if (!string.IsNullOrEmpty(name))
                    {
                        names.Add(name);
                    }
                }
            }

            membership.Add(new KeyValuePair<int, List<string>>(teamId, names));
        }

        return membership;
    }
}
