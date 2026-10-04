using UnityEngine;

public class DisposeFlare : BaseDisposeCallback
{
    public Light Light;

    public ParticleSystem Smoke;

    public float DisposeOffset;

    public override void DisposeItem(float currentTime)
    {
        if (Light != null)
        {
            Light.enabled = !(currentTime <= DisposeOffset);
        }

        if (Smoke != null)
        {
            var emission = Smoke.emission;
            emission.enabled = !(currentTime <= DisposeOffset);
        }
    }
}
