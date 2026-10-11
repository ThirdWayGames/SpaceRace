using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Mutations/Scope Sway")]
[Serializable]
public class ScopeSwayMutation : RepeatableMutation
{
    public ScopeSwaySettings Settings;

    [Header("Added on top of the profile while this mutation is applied")]
    public float SwayAmountAdjustment;

    public float BreathHoldReductionAdjustment;

    public float SwayPenaltyAdjustment;

    public float SwayReturnSecondsAdjustment;

    public float BreathHoldSecondsAdjustment;

    ScopeSwaySettings previous;

    ScopeSwaySettings runtimeCopy;

    bool applied;

    public override void CureMutation(GameObject parentGameObject, GameObject gameObject)
    {
        base.CureMutation(parentGameObject, gameObject);
        if (!isCured || !ReinstateValuesAfterCure || !applied)
        {
            return;
        }

        var rifle = parentGameObject != null ? parentGameObject.GetComponentInChildren<SniperRifle>(true) : null;
        if (rifle != null)
        {
            rifle.Profile = previous;
        }

        if (runtimeCopy != null)
        {
            Destroy(runtimeCopy);
            runtimeCopy = null;
        }

        applied = false;
    }

    protected override void ExecutionTick(GameObject parentGameObject)
    {
        if (applied || parentGameObject == null || Settings == null)
        {
            return;
        }

        var rifle = parentGameObject.GetComponentInChildren<SniperRifle>(true);
        if (rifle == null)
        {
            return;
        }

        previous = rifle.Profile;
        rifle.Profile = BuildProfile();
        applied = true;
    }

    ScopeSwaySettings BuildProfile()
    {
        if (!HasAdjustment())
        {
            return Settings;
        }

        runtimeCopy = Instantiate(Settings);
        runtimeCopy.name = name + "Profile";
        runtimeCopy.SwayAmount += SwayAmountAdjustment;
        runtimeCopy.BreathHoldReduction += BreathHoldReductionAdjustment;
        runtimeCopy.SwayPenalty += SwayPenaltyAdjustment;
        runtimeCopy.SwayReturnSeconds += SwayReturnSecondsAdjustment;
        runtimeCopy.BreathHoldSeconds += BreathHoldSecondsAdjustment;
        return runtimeCopy;
    }

    bool HasAdjustment()
    {
        return Mathf.Abs(SwayAmountAdjustment) > 0.0001f
            || Mathf.Abs(BreathHoldReductionAdjustment) > 0.0001f
            || Mathf.Abs(SwayPenaltyAdjustment) > 0.0001f
            || Mathf.Abs(SwayReturnSecondsAdjustment) > 0.0001f
            || Mathf.Abs(BreathHoldSecondsAdjustment) > 0.0001f;
    }
}
