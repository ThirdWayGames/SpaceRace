using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(menuName = "Mutations/Timed")]
[Serializable]
/*
 * The timed mutation defines that after a defined period of time in seconds 
 * mutations will then be cured and then applied to the affected target.
 */
public class TimedMutation : BaseMutation
{
    [Header("Exec Timings")]
    public bool RandomiseExec;

    public int ExecTimerMin;

    public int ExecTimerMax;

    [Header("Mutations")]
    // The mutations to cure from target when the mutation is triggered.
    public List<ScriptableObject> MutationsToCureOnTrigger;

    // The mutations to apply to the target when the mutation is triggered.
    public List<ScriptableObject> MutationsToApplyOnTrigger;

    public override void Initialise(GameObject parentGameObject)
    {
        base.Initialise(parentGameObject);
        LastExecution = Time.time;
    }

    public override void ExecuteMutation(GameObject parentGameObject)
    {
        // If the mutation is not cured.
        if (!isCured)
        {
            // If execution has occrred once already and its repeatable
            if (CurrentExecCount > 0)
            {
                // If we are due to execute
                if ((Time.time - LastExecution) >= ExecutionTime)
                {
                    try
                    {
                        // Always execute the mutation
                        ExecutionTick(parentGameObject);
                    }
                    catch (Exception ex)
                    {
                        // If an error occured, log it.
                        Debug.LogException(ex);
                    } finally
                    {
                        // Finally, regardless of the outcome of the exec tick, set the execution time.
                        LastExecution = Time.time;

                        if (RandomiseExec)
                        {
                            ExecutionTime = UnityEngine.Random.Range(ExecTimerMin, ExecTimerMax);
                        }
                    }
                }
            }
            else
            {
                base.ExecuteMutation(parentGameObject);
            }
        }
    }

    protected override void ExecutionTick(GameObject parentGameObject)
    {
        var mutationController = parentGameObject.GetComponent<MutationController>();

        if (mutationController == null)
        {
            throw new Exception(string.Format("Cant apply mutations to '{0}'. Missing mutation controller.", parentGameObject.name));
        }

        mutationController.CureMutation(MutationsToCureOnTrigger.Cast<BaseMutation>().ToList());
        mutationController.AddMutation(MutationsToApplyOnTrigger.Cast<BaseMutation>().ToList(), this.MutationTeamId);
    }
}