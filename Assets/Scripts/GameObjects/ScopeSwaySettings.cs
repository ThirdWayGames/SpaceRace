using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SpaceRace/Scope Sway")]
[Serializable]
public class ScopeSwaySettings : ScriptableObject
{
    [Header("Scope sway")]
    [Tooltip("How far the aim drifts, in pixels, at full sway.")]
    public float SwayAmount = 28f;

    [Tooltip("How fast the drift cycles.")]
    public float SwaySpeed = 1.6f;

    [Header("Breath hold")]
    [Tooltip("Fraction of sway removed while standing still and holding shift. 0.9 leaves 10 percent.")]
    public float BreathHoldReduction = 0.9f;

    [Tooltip("Seconds to ease into the breath-hold reduction.")]
    public float BreathSettleSeconds = 0.5f;

    [Tooltip("Seconds the breath bar takes to fill.")]
    public float BreathHoldSeconds = 4f;

    [Header("Breath break")]
    [Tooltip("Sway multiplier when the breath breaks. 1.1 is 110 percent.")]
    public float SwayPenalty = 1.1f;

    [Tooltip("Seconds to ease the penalty back to normal sway.")]
    public float SwayReturnSeconds = 1.5f;

    [Header("Blocked shot")]
    [Tooltip("How dark the scope gets when the laser hits something before the cursor.")]
    public float BlockedShade = 0.62f;

    static ScopeSwaySettings fallback;

    public static ScopeSwaySettings Fallback
    {
        get
        {
            if (fallback == null)
            {
                fallback = CreateInstance<ScopeSwaySettings>();
                fallback.name = "ScopeSwayFallback";
            }

            return fallback;
        }
    }
}

public struct ScopeSwayState
{
    public float Breath;

    public float Recovery;

    public float Settle;

    public bool Recovering;
}

public static class ScopeSwayMath
{
    public static bool StandingStill(float horizontal, float vertical)
    {
        return horizontal > -0.01f && horizontal < 0.01f && vertical > -0.01f && vertical < 0.01f;
    }

    public static bool HoldingBreath(bool standingStill, bool shiftHeld, bool recovering)
    {
        return standingStill && shiftHeld && !recovering;
    }

    public static ScopeSwayState Step(ScopeSwayState state, bool holdingBreath, float delta, float breathSeconds, float returnSeconds, float settleSeconds)
    {
        if (delta < 0f)
        {
            delta = 0f;
        }

        if (state.Recovering)
        {
            var duration = returnSeconds < 0.05f ? 0.05f : returnSeconds;
            state.Recovery += delta / duration;
            if (state.Recovery >= 1f)
            {
                state.Recovering = false;
                state.Recovery = 1f;
                state.Breath = 0f;
                state.Settle = 0f;
            }
            else
            {
                state.Breath = 1f - state.Recovery;
            }

            return state;
        }

        if (!holdingBreath)
        {
            state.Breath = 0f;
            state.Settle = 0f;
            return state;
        }

        var settle = settleSeconds < 0.05f ? 0.05f : settleSeconds;
        state.Settle += delta / settle;
        if (state.Settle > 1f)
        {
            state.Settle = 1f;
        }

        var fill = breathSeconds < 0.05f ? 0.05f : breathSeconds;
        state.Breath += delta / fill;
        if (state.Breath >= 1f)
        {
            state.Breath = 1f;
            state.Recovering = true;
            state.Recovery = 0f;
            state.Settle = 0f;
        }

        return state;
    }

    public static float Multiplier(float breathHoldReduction, bool holding, bool recovering, float recovery01, float penalty, float settle01)
    {
        if (recovering)
        {
            var t = recovery01 < 0f ? 0f : (recovery01 > 1f ? 1f : recovery01);
            var pen = penalty < 1f ? 1f : penalty;
            return pen + ((1f - pen) * t);
        }

        if (holding)
        {
            var reduction = breathHoldReduction;
            if (reduction < 0f)
            {
                reduction = 0f;
            }

            if (reduction > 1f)
            {
                reduction = 1f;
            }

            var settle = settle01 < 0f ? 0f : (settle01 > 1f ? 1f : settle01);
            return 1f - (reduction * settle);
        }

        return 1f;
    }

    public static Vector2 Offset(float time, float amount, float speed)
    {
        if (amount < 0f)
        {
            amount = 0f;
        }

        if (speed < 0f)
        {
            speed = 0f;
        }

        var x = Mathf.Sin(time * speed) * amount;
        var y = Mathf.Sin((time * speed * 0.67f) + 1.3f) * amount * 0.72f;
        var drift = new Vector2(x, y);
        if (amount > 0f && drift.magnitude > amount)
        {
            drift = drift.normalized * amount;
        }

        return drift;
    }
}
