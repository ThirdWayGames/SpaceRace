using UnityEngine;

public enum TimedDestroyAction
{
    LocalDestroy,
    NetworkDestroy,
    WaitForOwner
}

public static class FlareLighting
{
    public const int MaxActiveLights = 4;

    public const int MaxParticles = 200;

    public const float MaxEmissionRate = 80f;

    public const float MaxParticleScreenSize = 0.5f;

    public const float LightClearance = 0.45f;

    /// <summary>
    /// Wider than the old range of 5, with a lower intensity than the old value of 10.
    /// The pool reaches farther and the center stays softer.
    /// </summary>
    public const float LightRange = 16f;

    public const float LightIntensity = 4f;

    public static void ApplyThrownLight(Light light)
    {
        if (light == null)
        {
            return;
        }

        light.range = LightRange;
        light.intensity = LightIntensity;
        light.shadows = LightShadows.None;
    }

    public static bool ShouldKeepLight(float timeRemaining, float disposeOffset)
    {
        return timeRemaining > disposeOffset;
    }

    public static bool IsGroundContact(float normalY)
    {
        return normalY >= 0.5f;
    }

    public static float LiftAlongNormal(float lightOffsetAlongNormal, float clearance)
    {
        var lift = clearance - lightOffsetAlongNormal;
        return lift > 0f ? lift : 0f;
    }

    public static bool[] ChooseActiveLights(bool[] wantsLightOldestFirst, int maxActive)
    {
        var allow = new bool[wantsLightOldestFirst.Length];
        var lit = 0;
        for (int i = wantsLightOldestFirst.Length - 1; i >= 0; i--)
        {
            allow[i] = wantsLightOldestFirst[i] && lit < maxActive;
            if (allow[i])
            {
                lit++;
            }
        }

        return allow;
    }

    /// <summary>
    /// Networked instances stay in the room buffer until the owner removes them.
    /// A local destroy, or a network destroy of a view that was never instantiated, leaves that buffer entry behind.
    /// </summary>
    public static TimedDestroyAction ChooseDestroy(bool inRoom, bool hasPhotonView, bool isMine, int instantiationId)
    {
        if (!inRoom || !hasPhotonView || instantiationId < 1)
        {
            return TimedDestroyAction.LocalDestroy;
        }

        return isMine ? TimedDestroyAction.NetworkDestroy : TimedDestroyAction.WaitForOwner;
    }
}

public static class FlareThrow
{
    public const float MaxDistanceMultiplier = 2f;

    public const float FullChargeSeconds = 1f;

    /// <summary>
    /// A flat throw spends a fixed time in the air, so travel distance scales with launch speed.
    /// No charge keeps today's distance. A full hold doubles it.
    /// </summary>
    public static float SpeedMultiplier(float heldSeconds)
    {
        if (heldSeconds <= 0f || MaxDistanceMultiplier <= 1f)
        {
            return 1f;
        }

        var charge = FullChargeSeconds <= 0f ? 1f : heldSeconds / FullChargeSeconds;
        if (charge > 1f)
        {
            charge = 1f;
        }

        return 1f + (MaxDistanceMultiplier - 1f) * charge;
    }
}
