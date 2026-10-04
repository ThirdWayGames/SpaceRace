using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects.Pathogens;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public class PathogenGun : Weapon
    {
        public ScriptableObject Pathogen;

        /// <summary>
        /// Spawns the bullet.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="position">The position.</param>
        /// <param name="rotation">The rotation.</param>
        /// <param name="aiBullet">If the bullet is from an ai.</param>
        /// <returns>
        /// The spawned bullet.
        /// </returns>
        protected override GameObject SpawnBullet(IPlayerController player, Vector3 position, Quaternion rotation)
        {
            GameObject spawnedBullet = null;
            if (BulletPrefab == null)
            {
                Debug.LogError(string.Format("No Bullet prefab assigned to weapon: '{0}'", ItemName()));
                return null;
            }

            // Determine which bullet prefab to use.
            var selectedFireModePrefab = GetBulletToSpawn();
            if (selectedFireModePrefab != null)
            {
                // Get the team id from the person firing the gun (assume 0 [AI])
                var teamComponent = player.GetComponent<TeamComponent>();
                var teamId = teamComponent != null ? teamComponent.TeamIdentifier : 0;

                // If we are in a network game.
                var bulletData = GenerateSpawnData(player);
                if (PhotonNetwork.inRoom)
                {
                    // Network spawn the bullet with the velocity/lifetime data.
                    spawnedBullet = PhotonNetwork.Instantiate(selectedFireModePrefab.name, position, rotation, 0, bulletData.ToOjectArray());
                }
                else
                {
                    // If we are not in a network game.
                    spawnedBullet = Instantiate(selectedFireModePrefab, position, rotation);
                    spawnedBullet.GetComponent<IBullet>().SetSpawnData(GenerateSpawnData(player));
                }
            }

            return spawnedBullet;
        }

        public override void Equip(IPlayerController player)
        {
            Debug.Log("No special affects.");
        }

        public override void Unequip(IPlayerController player)
        {
            Debug.Log("Pistol Cant be unequiped.");
        }

        public override void Animate(IPlayerController player)
        {
        }

        protected bool ApplyNewPathogen()
        {
            // Determine if the pathogen should be set for the bullet based on the delivery method and the fire mode.
            var applyPathogen = false;
            var pathogen = Pathogen as Pathogen;
            if (pathogen != null)
            {
                switch (CurrentFireMode)
                {
                    case Enums.FireMode.Single:
                        {
                            applyPathogen = pathogen.DeliveryType.ToString().Contains(Enums.PathogenInfectionType.Blood.ToString());
                        }
                        break;
                    case Enums.FireMode.Cloud:
                        {
                            applyPathogen = pathogen.DeliveryType.ToString().Contains(Enums.PathogenInfectionType.Airborne.ToString());
                        }
                        break;
                    case Enums.FireMode.Beam:
                        {
                            applyPathogen = pathogen.DeliveryType.ToString().Contains(Enums.PathogenInfectionType.Contact.ToString());
                        }
                        break;
                }
            }

            return applyPathogen;
        }

        protected override ISpawnData GenerateSpawnData(IPlayerController player)
        {
            var bulletData = (BulletData)base.GenerateSpawnData(player);

            return new PathogenBulletData
            {
                BulletVelocity = bulletData.BulletVelocity,
                BulletLifetime = bulletData.BulletLifetime,
                ShooterId = bulletData.ShooterId,
                ParentId = bulletData.ParentId,
                PathogenName = ApplyNewPathogen() ? ((Pathogen)Pathogen).name : null
            };
        }
    }
}