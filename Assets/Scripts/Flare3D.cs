using UnityEngine;

namespace Assets.Scripts
{
    public class Flare3D : Bullet3D
    {
        public float LightClearance = FlareLighting.LightClearance;

        bool settled;

        public override void OnCollisionEnter(Collision collision)
        {
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
