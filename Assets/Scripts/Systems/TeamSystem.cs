using System.Linq;
using Unity.Entities;

public class TeamSystem : ComponentSystem {

    public struct Group
    {
        public TeamComponent Team;
    }

    protected override void OnUpdate()
    {
        foreach (var entity in GetEntities<Group>())
        {
            if (PhotonNetwork.inRoom && entity.Team.TeamIdentifier == 0)
            {
                var photonView = entity.Team.gameObject.GetComponent<PhotonView>();

                if (photonView != null)
                {
                    var photonPlayer = PhotonNetwork.playerList.FirstOrDefault(x => x.ID == photonView.ownerId);

                    if (photonPlayer != null)
                    {
                        if (photonPlayer.CustomProperties != null && photonPlayer.CustomProperties.Count > 0)
                        {
                            var teamIdProperty = photonPlayer.CustomProperties[PlayerManager3D.PlayerTeamPrefKey];

                            if (teamIdProperty != null)
                            {
                                //// entity.Team.TeamIdentifier = (int)teamIdProperty;
                            }
                        }
                    }
                }
            }
        }
    }
}
