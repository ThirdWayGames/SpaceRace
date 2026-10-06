using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public static class FlarePlayerPassThrough
    {
        static readonly List<Collider> flareColliders = new List<Collider>();

        static readonly HashSet<long> ignoredPairs = new HashSet<long>();

        public static void RegisterFlare(Collider flare)
        {
            if (flare == null || flareColliders.Contains(flare))
            {
                return;
            }

            flareColliders.Add(flare);
            var players = Object.FindObjectsOfType<BasePlayerController3D>();
            for (int i = 0; i < players.Length; i++)
            {
                Ignore(flare, players[i]);
            }
        }

        public static void UnregisterFlare(Collider flare)
        {
            if (flare == null)
            {
                return;
            }

            flareColliders.Remove(flare);
        }

        public static void RegisterPlayer(BasePlayerController3D player)
        {
            if (player == null)
            {
                return;
            }

            for (int i = flareColliders.Count - 1; i >= 0; i--)
            {
                var flare = flareColliders[i];
                if (flare == null)
                {
                    flareColliders.RemoveAt(i);
                    continue;
                }

                Ignore(flare, player);
            }
        }

        public static void Reset()
        {
            flareColliders.Clear();
            ignoredPairs.Clear();
        }

        public static void NoteIgnored(Collider flare, Collider other)
        {
            if (flare == null || other == null)
            {
                return;
            }

            ignoredPairs.Add(PairKey(flare, other));
        }

        public static bool WasIgnored(Collider flare, Collider other)
        {
            if (flare == null || other == null)
            {
                return false;
            }

            return ignoredPairs.Contains(PairKey(flare, other));
        }

        static void Ignore(Collider flare, BasePlayerController3D player)
        {
            var playerColliders = player.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < playerColliders.Length; i++)
            {
                var playerCollider = playerColliders[i];
                if (playerCollider == null || playerCollider == flare || !playerCollider.enabled || !flare.enabled)
                {
                    continue;
                }

                Physics.IgnoreCollision(flare, playerCollider, true);
                ignoredPairs.Add(PairKey(flare, playerCollider));
            }
        }

        static long PairKey(Collider a, Collider b)
        {
            var left = a.GetInstanceID();
            var right = b.GetInstanceID();
            if (left > right)
            {
                var swap = left;
                left = right;
                right = swap;
            }

            return ((long)left << 32) | (uint)right;
        }
    }

    public class Flare3D : Bullet3D
    {
        public float LightClearance = FlareLighting.LightClearance;

        bool settled;

        Collider bodyCollider;

        public override void Awake()
        {
            base.Awake();
            bodyCollider = GetComponent<Collider>();
        }

        void OnEnable()
        {
            FlarePlayerPassThrough.RegisterFlare(bodyCollider != null ? bodyCollider : GetComponent<Collider>());
        }

        void OnDisable()
        {
            FlarePlayerPassThrough.UnregisterFlare(bodyCollider != null ? bodyCollider : GetComponent<Collider>());
        }

        public override void OnCollisionEnter(Collision collision)
        {
            if (collision.collider != null && collision.collider.GetComponentInParent<BasePlayerController3D>() != null)
            {
                var mine = bodyCollider != null ? bodyCollider : GetComponent<Collider>();
                if (mine != null && collision.collider.enabled && mine.enabled)
                {
                    Physics.IgnoreCollision(mine, collision.collider, true);
                    FlarePlayerPassThrough.NoteIgnored(mine, collision.collider);
                }

                return;
            }

            if (settled || collision.contacts == null || collision.contacts.Length == 0)
            {
                return;
            }

            var contact = collision.contacts[0];
            if (!FlareLighting.IsGroundContact(contact.normal.y))
            {
                return;
            }

            settled = true;
            var body = GetComponent<Rigidbody>();
            if (body != null)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            var light = GetComponentInChildren<Light>();
            var lightPosition = light != null ? light.transform.position : transform.position;
            var normal = contact.normal.sqrMagnitude > 0.0001f ? contact.normal.normalized : Vector3.up;
            var offset = Vector3.Dot(lightPosition - contact.point, normal);
            var lift = FlareLighting.LiftAlongNormal(offset, LightClearance);
            if (lift > 0f)
            {
                transform.position += normal * lift;
            }
        }
    }
}
