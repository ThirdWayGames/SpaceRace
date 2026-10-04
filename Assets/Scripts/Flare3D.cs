using UnityEngine;

namespace Assets.Scripts
{
    public class Flare3D : Bullet3D
    {
        public override void OnCollisionEnter(Collision collision)
        {
            Debug.Log(string.Format("{0} collided with {1}", collision.contacts[0].thisCollider.name, collision.contacts[0].otherCollider.name));
        }
    }
}