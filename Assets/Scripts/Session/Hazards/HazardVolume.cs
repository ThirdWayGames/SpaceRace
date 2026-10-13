using Assets.Scripts;
using UnityEngine;

/// <summary>
/// Away-site trigger. Place this on a trigger collider off the deploy spawn pad so walking onto the pad does not infect.
/// </summary>
public class HazardVolume : MonoBehaviour
{
    public string PathogenResourceName = "Anthrax";

    void OnTriggerEnter(Collider other)
    {
        if (other == null || string.IsNullOrEmpty(PathogenResourceName))
        {
            return;
        }

        var player = other.GetComponent<BasePlayerController3D>();
        if (player == null)
        {
            player = other.GetComponentInParent<BasePlayerController3D>();
        }

        if (player == null)
        {
            return;
        }

        var view = player.GetComponent<PhotonView>();
        if (view == null)
        {
            view = player.GetComponentInParent<PhotonView>();
        }

        if (PhotonNetwork.inRoom && (view == null || !view.isMine))
        {
            return;
        }

        var mutation = player.GetComponent<MutationController>();
        if (mutation == null)
        {
            mutation = player.GetComponentInParent<MutationController>();
        }

        if (mutation == null)
        {
            return;
        }

        // Local call of the owner-gated PunRPC. PhotonTargets.Others would skip this client.
        mutation.AddPathogen(PathogenResourceName, player.GetTeamId());
    }
}
