using System.Collections.Generic;
using UnityEngine;

public static class FlareLightBudget
{
    static readonly List<DisposeFlare> flares = new List<DisposeFlare>();

    public static void Register(DisposeFlare flare)
    {
        if (flare == null || flares.Contains(flare))
        {
            return;
        }

        flares.Add(flare);
        Apply();
    }

    public static void Unregister(DisposeFlare flare)
    {
        if (flare == null)
        {
            return;
        }

        if (flares.Remove(flare))
        {
            Apply();
        }
    }

    public static void Reset()
    {
        flares.Clear();
    }

    public static void Apply()
    {
        for (int i = flares.Count - 1; i >= 0; i--)
        {
            if (flares[i] == null)
            {
                flares.RemoveAt(i);
            }
        }

        var wantsLight = new bool[flares.Count];
        for (int i = 0; i < flares.Count; i++)
        {
            wantsLight[i] = flares[i].WantsLight;
        }

        var allow = FlareLighting.ChooseActiveLights(wantsLight, FlareLighting.MaxActiveLights);
        for (int i = 0; i < flares.Count; i++)
        {
            flares[i].ApplyLight(allow[i]);
        }
    }
}

public class DisposeFlare : BaseDisposeCallback
{
    public Light Light;

    public ParticleSystem Smoke;

    public float DisposeOffset;

    float remaining;

    bool wantsLight = true;

    bool smokeStopped;

    public bool WantsLight
    {
        get { return wantsLight; }
    }

    void Awake()
    {
        FlareLighting.ApplyThrownLight(Light);

        var destroyMe = GetComponent<DestroyMe>();
        remaining = destroyMe != null ? destroyMe.DestroyTimer : 0f;
        ClampSmoke();
    }

    void OnEnable()
    {
        FlareLightBudget.Register(this);
        ApplyState(remaining);
    }

    void OnDisable()
    {
        FlareLightBudget.Unregister(this);
    }

    void Update()
    {
        if (!wantsLight)
        {
            return;
        }

        remaining -= Time.deltaTime;
        if (FlareLighting.ShouldKeepLight(remaining, DisposeOffset))
        {
            return;
        }

        ApplyState(remaining);
    }

    public override void DisposeItem(float currentTime)
    {
        remaining = currentTime;
        ApplyState(currentTime);
    }

    public void ApplyLight(bool budgetAllows)
    {
        if (Light == null)
        {
            return;
        }

        var enabled = budgetAllows && wantsLight;
        if (Light.enabled != enabled)
        {
            Light.enabled = enabled;
        }
    }

    void ApplyState(float currentTime)
    {
        wantsLight = FlareLighting.ShouldKeepLight(currentTime, DisposeOffset);
        StopSmoke(wantsLight);
        FlareLightBudget.Apply();
    }

    void StopSmoke(bool emit)
    {
        if (Smoke == null)
        {
            return;
        }

        var emission = Smoke.emission;
        if (emission.enabled != emit)
        {
            emission.enabled = emit;
        }

        if (!emit && !smokeStopped)
        {
            Smoke.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            smokeStopped = true;
        }
    }

    void ClampSmoke()
    {
        if (Smoke == null)
        {
            return;
        }

        var main = Smoke.main;
        if (main.maxParticles > FlareLighting.MaxParticles)
        {
            main.maxParticles = FlareLighting.MaxParticles;
        }

        var emission = Smoke.emission;
        var rate = emission.rateOverTime;
        if (rate.mode == ParticleSystemCurveMode.Constant && rate.constant > FlareLighting.MaxEmissionRate)
        {
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(FlareLighting.MaxEmissionRate);
        }

        var smokeRenderer = Smoke.GetComponent<ParticleSystemRenderer>();
        if (smokeRenderer != null && smokeRenderer.maxParticleSize > FlareLighting.MaxParticleScreenSize)
        {
            smokeRenderer.maxParticleSize = FlareLighting.MaxParticleScreenSize;
        }
    }
}
