using Assets.Scripts.ScriptableObjects.Pathogens;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathogenLibrary : Photon.PunBehaviour
{
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
                        AddPathogen(pathLibInt.TeamId, pathgen.name, true);
                    }

                    foreach (var pathgen in pathLibInt.KnownPathogens)
                    {
                        AddPathogen(pathLibInt.TeamId, pathgen.name);
                    }
                }
            }
        }
    }

    public void AddPathogenLocally(int teamId, string pathogenName, bool isCure = false)
    {
        if (this.photonView != null && PhotonNetwork.inRoom)
        {
            this.photonView.RPC("AddPathogen", PhotonNetworkSettings.DefaultRPCNetworkTarget, new object[] { teamId, pathogenName, isCure });
        }
        else
        {
            this.AddPathogen(teamId, pathogenName, isCure);
        }
    }

    [PunRPC]
    public void AddPathogen(int teamId, string pathogenName, bool isCure = false)
    {
        // Determine which list to add to.
        var pathogenList = isCure ? TeamPathogenCures : TeamKnownPathogens;

        // If my team doesn't currently have a list.
        if (!pathogenList.Any(x => x.Key == teamId))
        {
            // Add one.
            pathogenList.Add(new KeyValuePair<int, List<string>>(teamId, new List<string>()));
        }

        // Get the current list for the team
        var pathogens = pathogenList.FirstOrDefault(x => x.Key == teamId);

        // If the pathogen is not already in the list.
        if (!pathogens.Value.Contains(pathogenName))
        {
            // Add it.
            pathogens.Value.Add(pathogenName);
            Debug.LogFormat("Adding {0} for team {1}", pathogenName, teamId);
        }
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
}
