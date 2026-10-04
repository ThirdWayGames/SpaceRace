using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Although this is called a MutationApplicator it's actual function is a ZonalMutationManager
/// - It is not being renamed to maintain current code stability.
/// </summary>
public class MutationApplicator : TriggerTargetManager
{
    public int TeamId;

    [Header("Mutations To Apply")]
    public List<ScriptableObject> MutationsToApplyOnEnter;
    public List<ScriptableObject> MutationsToApplyOnExit;

    [Header("Mutations To Cure")]
    public List<ScriptableObject> MutationsToCureOnEnter;
    public List<ScriptableObject> MutationsToCureOnExit;

    // get the MutationController from the player and add all mutations.
    public override void OnTriggerStay(Collider other)
    {
        if (other != null)
        {
            if (!TriggerList.Contains(other))
            {
                TriggerList.Add(other);

                var mutationCont = other.gameObject.transform.GetComponentInParent<MutationController>();
                if (mutationCont != null)
                {
                    // Cure any mutations in the cure list on enter.
                    mutationCont.CureMutation(MutationsToCureOnEnter.Cast<BaseMutation>().ToList());

                    // Apply any mutations in the apply list on enter.
                    mutationCont.AddMutation(MutationsToApplyOnEnter.Cast<BaseMutation>().ToList(), TeamId);
                }
            }
        }
    }

    public override void OnTriggerExit(Collider other)
    {
        if (other != null)
        {
            if (TriggerList.Contains(other))
            {
                TriggerList.Remove(other);
            }

            var mutationCont = other.gameObject.transform.GetComponentInParent<MutationController>();
            if (mutationCont != null)
            {
                // Cure any mutations in the cure list on enter.
                mutationCont.CureMutation(MutationsToCureOnExit.Cast<BaseMutation>().ToList());

                // Apply any mutations in the apply list on enter.
                mutationCont.AddMutation(MutationsToApplyOnExit.Cast<BaseMutation>().ToList(), TeamId);
            }
        }
    }
}
