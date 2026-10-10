using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Components;
using Assets.Scripts.GameObjects;
using UnityEngine;

public class GrenadeRound : Bullet3D
{
    public float BlastRadius = 3.4f;

    public AudioClip ExplosionSound;

    bool exploded;

    bool armed;

    public override void Start()
    {
        base.Start();
        var body = GetComponent<Collider>();
        if (body != null)
        {
            body.enabled = false;
        }

        Invoke("Arm", 0.25f);
        Invoke("Explode", 4f);
    }

    void Arm()
    {
        armed = true;
        var body = GetComponent<Collider>();
        if (body != null)
        {
            body.enabled = true;
        }
    }

    public override void OnCollisionEnter(Collision collision)
    {
        if (!armed || exploded)
        {
            return;
        }

        Explode();
    }

    void Explode()
    {
        if (exploded)
        {
            return;
        }

        exploded = true;
        CancelInvoke("Explode");
        Detonate();
        RemoveBullet(0f, null);
    }

    void Detonate()
    {
        if (ExplosionSound != null)
        {
            AudioSource.PlayClipAtPoint(ExplosionSound, transform.position, 1f);
        }

        var flash = new GameObject("GrenadeFlash");
        flash.transform.position = transform.position + Vector3.up * 0.4f;
        var light = flash.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.46f, 0.16f);
        light.range = BlastRadius * 2.2f;
        light.intensity = 6f;
        Destroy(flash, 0.35f);

        if (!ShouldApplyDamage())
        {
            return;
        }

        var hits = Physics.OverlapSphere(transform.position, BlastRadius);
        var seen = new HashSet<int>();
        var bulletData = SpawnData as BulletData;
        var shooter = bulletData != null ? bulletData.ShooterId : 0;
        for (var i = 0; i < hits.Length; i++)
        {
            var controller = hits[i].GetComponentInParent<MutationController>();
            if (controller == null || !seen.Add(controller.GetInstanceID()))
            {
                continue;
            }

            if (!GrenadeBlast.CanDamage(PhotonNetwork.inRoom, hits[i].GetComponentInParent<PhotonView>()))
            {
                continue;
            }

            if (MutationsToApply == null || MutationsToApply.Count == 0)
            {
                continue;
            }

            var mutations = MutationsToApply.Cast<BaseMutation>().Where(item => item != null).ToList();
            controller.AddMutation(mutations, shooter);
        }
    }

    bool ShouldApplyDamage()
    {
        if (!PhotonNetwork.inRoom)
        {
            return true;
        }

        var view = GetComponent<PhotonView>();
        return view == null || view.isMine;
    }
}

public static class GrenadeBlast
{
    public static bool Inside(Vector3 center, Vector3 point, float radius)
    {
        if (radius < 0f)
        {
            return false;
        }

        return (point - center).sqrMagnitude <= radius * radius;
    }

    public static bool CanDamage(bool inRoom, PhotonView targetView)
    {
        if (!inRoom || targetView == null)
        {
            return true;
        }

        return targetView.isMine;
    }
}
