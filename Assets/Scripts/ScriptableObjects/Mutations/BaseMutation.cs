using System;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class BaseMutation : ScriptableObject, IBaseMutation
{
    [Tooltip("This will be the execution delay")]
    public float ExecutionTime;

    [Header("Cure Process")]
    public bool CureAfterExecution = true;

    public bool VacinateAfterCure;

    [Tooltip("This will reverse the mutation affect by 1 execution cycle on the target component when cured.")]
    public bool ReinstateValuesAfterCure;

    public bool DelayCureByExecTime = true;

    public float MutationDuration;

    protected int MutationTeamId;

    protected float TimeMutationApplied;

    protected float LastExecution;

    protected int CurrentExecCount = 0;

    protected bool isCured = false;

    protected bool isInitialised;

    protected GameObject ParentGameObject;

    public virtual void CureMutation(GameObject parentGameObject, GameObject gameObject)
    {
        // Mutation duration has passed and is now cured.
        isCured = true;
    }

    public virtual void Initialise(GameObject parentGameObject)
    {
        isCured = false;
        isInitialised = true;
        TimeMutationApplied = Time.time;
        ParentGameObject = parentGameObject;
    }

    protected virtual bool IsReadyToCure()
    {
        return Time.time >= TimeMutationApplied + MutationDuration;
    }

    public virtual bool IsCured()
    {
        return isCured;
    }

    public virtual bool IsInitialised()
    {
        return isInitialised;
    }

    public virtual Transform GetMeshTransform(GameObject parentGameObject)
    {
        var mesh = parentGameObject.transform.Find("Body");
        if (mesh == null)
        {
            mesh = parentGameObject.transform.Find("VFX/Armature");
        }

        return mesh;
    }

    /// <summary>
    /// If the mutation has any requirements to be applied these can be checked here.
    /// If they are not met then the mutation will not be applied.
    /// </summary>
    /// <param name="parentToCheck">The GameObject that the mutation controller is on.</param>
    /// <returns>True if the mutation can be applied to the GO that the Mutation Controller is on.</returns>
    public virtual bool CanApplyMutation(GameObject parentToCheck)
    {
        return true;
    }

    public virtual void ExecuteMutation(GameObject parentGameObject)
    {
        // If we have not executed yet.
        if (CurrentExecCount == 0 && (Time.time - LastExecution) >= ExecutionTime)
        {
            // Execute
            ExecutionTick(parentGameObject);

            // Increment the exectution count.
            CurrentExecCount += 1;
        }
    }

    public virtual bool CanCureAfterExec()
    {
        // If it can be cured after exec and has been executed at least once.
        return CureAfterExecution && CurrentExecCount > 0 && IsReadyToCure();
    }

    public virtual GameObject GetParentObject()
    {
        return ParentGameObject;
    }

    public void SetMutationTeamId(int mutationTeamId)
    {
        MutationTeamId = mutationTeamId;
    }

    protected abstract void ExecutionTick(GameObject parentGameObject);
}