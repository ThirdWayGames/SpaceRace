using UnityEngine;
using System.Linq;
using Assets.Scripts.ScriptableObjects.Pathogens;
using System.Collections.Generic;

public class PathogenApplicator : TriggerTargetManager
{
    public int TeamId;

    public bool FriendlyFire = true;

    [Header("Pathogen")]
    public ScriptableObject Pathogen;

    // get the MutationController from the player and add all mutations.
    public override void OnTriggerStay(Collider other)
    {
        // If the trigger list doesn't contain the other collider
        if (!TriggerList.Contains(other))
        {
            // Add them
            TriggerList.Add(other);

            // Get the team component.
            var teamIdComp = other.gameObject.transform.GetComponentInParent<TeamComponent>();
            var teamId = teamIdComp == null ? 0 : teamIdComp.TeamIdentifier;

            // Get the mutation controller from the other collider
            var mutationCont = other.gameObject.transform.GetComponentInParent<MutationController>();

            // If the mutation controller is not null and the team id of the other collider does not match this pathogen applicator.
            // in short, this pathogen cloud was NOT created by my team.
            if (mutationCont != null && (FriendlyFire || teamId != TeamId))
            {
                // Apply any mutations in the apply list on enter.
                var pathogenToAdd = Pathogen as Pathogen;

                if (pathogenToAdd != null)
                {
                    mutationCont.AddPathogen(pathogenToAdd, TeamId);
                }
            }
        }
    }
}
