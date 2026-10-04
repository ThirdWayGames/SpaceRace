using Assets.Scripts.Components;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Mutations/Component/Stat Modifer")]
[Serializable]
public class StatModMutation : RepeatableMutation
{
    protected float? NonMutatedConstantValue = null;

    protected float? NonMutatedCurrentValue = null;

    protected MutatableComponent ComponentToAffect;

    [Header("Statistic To Mutate")]
    public string ComponentTypeToAffect;

    [Header("Allow value to fall below zero")]
    public bool MutateBelowZero = true;

    public float ConstantValueAdjustment;

    public float RunningValueAdjustment;

    public override void CureMutation(GameObject parentGameObject, GameObject gameObject)
    {
        base.CureMutation(parentGameObject, gameObject);

        if (isCured)
        {
            StatModMutation tmpCureMut = null;

            // If we have executed the mutation at least once.
            if (CurrentExecCount > 0 && ReinstateValuesAfterCure)
            {
                if (ExecutionTime > 0 && DelayCureByExecTime)
                {
                    // Create a temp mutation to re-apply the values after a delay
                    tmpCureMut = ScriptableObject.CreateInstance<StatModMutation>();
                    tmpCureMut.name = string.Format("[TmpCure] {0}", this.name);
                    tmpCureMut.ComponentTypeToAffect = this.ComponentTypeToAffect;
                    tmpCureMut.CureAfterExecution = true;
                    tmpCureMut.ExecutionTime = this.ExecutionTime;
                    tmpCureMut.ReinstateValuesAfterCure = false;
                    tmpCureMut.ComponentToAffect = this.ComponentToAffect;
                    tmpCureMut.ConstantValueAdjustment = (ComponentToAffect.MaxValue - ConstantValueAdjustment) * 2;
                    tmpCureMut.RunningValueAdjustment = (ComponentToAffect.CurrentValue - RunningValueAdjustment) * 2;
                }
                else
                {
                    ComponentToAffect.MaxValue -= ConstantValueAdjustment;
                    ComponentToAffect.CurrentValue -= RunningValueAdjustment;
                }
            }

            // If we have created a temp mutation cure (due to initial delay).
            if (tmpCureMut != null)
            {
                // Add it to the mutation controller
                var mutationContoller = parentGameObject.GetComponent<MutationController>();
                if (mutationContoller != null)
                {
                    mutationContoller.AddMutation(tmpCureMut, this.MutationTeamId);
                }
            }
        }
    }

    public override void Initialise(GameObject parentGameObject)
    {
        var component = parentGameObject.GetComponent(ComponentTypeToAffect);

        // If no component was found
        if (component == null)
        {
            Debug.LogError(string.Format("'{0}' is not a component of {1}", ComponentTypeToAffect, parentGameObject.name));
        }
        
        // Cast the found component to a MutableComponent;
        ComponentToAffect = component as MutatableComponent;

        // If the compenent is not mutable.
        if (ComponentToAffect == null)
        {
            Debug.LogError(string.Format("'{0}' is not a component of {1}", ComponentTypeToAffect, parentGameObject.name));
        }
        else
        {
            if (NonMutatedConstantValue == null)
            {
                // Store the original value.
                NonMutatedConstantValue = ComponentToAffect.MaxValue;
            }

            if (NonMutatedCurrentValue == null)
            {
                // Store the original value.
                NonMutatedCurrentValue = ComponentToAffect.CurrentValue;
            }
        }

        isCured = false;
        isInitialised = true;
    }

    public override bool CanApplyMutation(GameObject parentToCheck)
    {
        var result = base.CanApplyMutation(parentToCheck);
        result = parentToCheck.GetComponent(ComponentTypeToAffect) != null;
        return result;
    }

    protected override void ExecutionTick(GameObject parentGameObject)
    {
        if (ConstantValueAdjustment != 0f)
        {
            ComponentToAffect.MaxValue = Mathf.Clamp(ComponentToAffect.MaxValue + ConstantValueAdjustment, MutateBelowZero ? ComponentToAffect.MaxValue + ConstantValueAdjustment : 0, ComponentToAffect.MaxValue + ConstantValueAdjustment);
        }

        // If the Max value is less than the current value, for example if we have reversed the movement speeds (running is -6[max] and walking is -4(current) then we need to reverse the clamp)
        if (RunningValueAdjustment != 0f)
        {
            if (ComponentToAffect.MaxValue < 0f)
            {
                // Reverse clamp
                ComponentToAffect.CurrentValue = Mathf.Clamp(ComponentToAffect.CurrentValue + RunningValueAdjustment, ComponentToAffect.MaxValue, MutateBelowZero ? ComponentToAffect.CurrentValue + RunningValueAdjustment : 0);
            }
            else
            {
                // Normal clamp
                ComponentToAffect.CurrentValue = Mathf.Clamp(ComponentToAffect.CurrentValue + RunningValueAdjustment, MutateBelowZero ? ComponentToAffect.CurrentValue + RunningValueAdjustment : 0, ComponentToAffect.MaxValue);
            }
        }
    }
}