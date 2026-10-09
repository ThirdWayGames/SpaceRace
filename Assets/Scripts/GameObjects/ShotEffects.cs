using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    public float Life = 0.08f;

    float age;
    Light glow;
    float fullIntensity;

    void Awake()
    {
        glow = GetComponent<Light>();
        fullIntensity = glow != null ? glow.intensity : 0f;
    }

    void Update()
    {
        age += Time.deltaTime;
        if (glow != null)
        {
            var remain = 1f - Mathf.Clamp01(age / Life);
            glow.intensity = fullIntensity * remain;
        }

        if (age >= Life)
        {
            Destroy(gameObject);
        }
    }
}

public static class ShotEffects
{
    public static void Flash(Vector3 position, Vector3 direction, string weaponName)
    {
        var color = ColorFor(weaponName);
        var flash = new GameObject("MuzzleFlash");
        flash.transform.position = position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            flash.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        var light = flash.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = 3.2f;
        light.intensity = 3.4f;
        light.shadows = LightShadows.None;
        flash.AddComponent<MuzzleFlash>();

        var particles = flash.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.loop = false;
        main.playOnAwake = false;
        main.duration = 0.12f;
        main.startLifetime = 0.1f;
        main.startSpeed = 7f;
        main.startSize = 0.07f;
        main.startColor = color;
        main.maxParticles = 18;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)12) });

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 14f;
        shape.radius = 0.02f;

        var renderer = particles.GetComponent<ParticleSystemRenderer>();
        var material = FlashMaterial();
        if (material != null)
        {
            renderer.sharedMaterial = material;
        }

        particles.Play();
    }

    public static void Present(GameObject shot)
    {
        if (shot == null)
        {
            return;
        }

        var color = ColorFor(shot.name);
        var trails = shot.GetComponentsInChildren<TrailRenderer>();
        for (var i = 0; i < trails.Length; i++)
        {
            var trail = trails[i];
            if (trail.time < 0.18f)
            {
                trail.time = 0.18f;
            }

            if (trail.widthMultiplier < 0.08f)
            {
                trail.widthMultiplier = 0.08f;
            }

            trail.emitting = true;
        }

        if (shot.GetComponentInChildren<Light>() != null)
        {
            return;
        }

        var lightObject = new GameObject("ShotLight");
        lightObject.transform.SetParent(shot.transform, false);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = 2.6f;
        light.intensity = 1.8f;
        light.shadows = LightShadows.None;
    }

    static Material flashMaterial;

    static Material FlashMaterial()
    {
        if (flashMaterial != null)
        {
            return flashMaterial;
        }

        var shader = Shader.Find("Particles/Additive");
        if (shader == null)
        {
            return null;
        }

        flashMaterial = new Material(shader);
        return flashMaterial;
    }

    public static Color ColorFor(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return new Color(1f, 0.62f, 0.22f);
        }

        if (name.IndexOf("Energy") >= 0)
        {
            return new Color(0.25f, 0.9f, 0.95f);
        }

        if (name.IndexOf("Heal") >= 0 || name.IndexOf("Medi") >= 0)
        {
            return new Color(0.35f, 0.95f, 0.48f);
        }

        if (name.IndexOf("Pathogen") >= 0)
        {
            return new Color(0.72f, 0.32f, 1f);
        }

        if (name.IndexOf("Stun") >= 0)
        {
            return new Color(0.55f, 0.78f, 1f);
        }

        return new Color(1f, 0.62f, 0.22f);
    }
}
