using UnityEngine;

public class PathogenRemover : TriggerTargetManager
{
    public int TeamId;

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

            // If the mutation controller is not null and the team id of the other collider matches this pathogen applicator.
            // in short, this pathogen cloud was created by my team.
            if (mutationCont != null && teamId == TeamId)
            {
                mutationCont.CurePathogens(teamId);
            }
        }
    }
}
