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
