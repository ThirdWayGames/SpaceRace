using UnityEngine;

namespace Assets.Scripts
{
    public class Flare3D : Bullet3D
    {
        public override void OnCollisionEnter(Collision collision)
        {
            // Land and burn. Bullet3D despawns on the first contact and would also log every bounce.
        }
    }
}
