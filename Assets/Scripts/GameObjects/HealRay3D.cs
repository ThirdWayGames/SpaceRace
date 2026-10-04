using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public class HealRay3D : Pistol3D
    {
        public override GameObject Fire(IPlayerController player, bool isRunning, float frameTiming, bool isAIBullet = false)
        {
            GameObject spawnedBullet = null;
            Running = isRunning;
            if (FireTimer >= FireDelay)
            {
                if (CurrentAmmo > 0)
                {
                    // reset the fire timer.
                    FireTimer = 0f;

                    // If we are running make the bullet less accurate by applying wider bullet spread.
                    Debug.Log(string.Format("Locating 'Muzzle' for {0} child of {1}", this.gameObject.transform.name,
                        this.gameObject.transform.parent != null ? this.gameObject.transform.parent.name : "No Parent"));
                    var gunPort = this.gameObject.transform.Find("Muzzle");
                    if (gunPort == null)
                    {
                        Debug.LogError("Can't find 'Muzzle' object to spawn bullet at.");
                        return null;
                    }

                    // Muzzle flash
                    Animate(player);

                    // Apply Bullet Spread.
                    var rot = ApplyBulletSpread(player, gunPort);

                    // Play sound
                    if (PhotonNetwork.inRoom && photonView != null)
                    {
                        PhotonNetwork.RPC(photonView, "PlayFireSound", PhotonNetworkSettings.DefaultRPCNetworkTarget, false, null);
                    }
                    else
                    {
                        PlayFireSound();
                    }

                    if (gunPort.childCount == 0)
                    {
                        // Fire the gun.
                        spawnedBullet = SpawnBullet(player, gunPort.position, Quaternion.Euler(rot));
                        spawnedBullet.transform.SetParent(gunPort.transform);
                    }
                }
                else
                {
                    // Play sound
                    if (PhotonNetwork.inRoom && photonView != null)
                    {
                        PhotonNetwork.RPC(photonView, "PlayEmptyClipSound", PhotonNetworkSettings.DefaultRPCNetworkTarget, false, null);
                    }
                    else
                    {
                        PlayEmptyClipSound();
                    }
                }
            }

            return spawnedBullet;
        }

        protected override GameObject SpawnBullet(IPlayerController player, Vector3 position, Quaternion rotation)
        {
            if (BulletPrefab == null)
            {
                Debug.LogError(string.Format("No Bullet prefab assigned to weapon: '{0}'", ItemName()));
                return null;
            }

            // Tell all the clients to spawn a bullet.
            var bulletObject = Instantiate(BulletPrefab, position, rotation);

            // Reduce ammo by 1;
            GameManager3D.ConsoleMsg(string.Format("Decrementing ammo by 1"));
            CurrentAmmo -= GameManager3D.instance.IsDebug ? 0 : 1;
            OnFireEvent.Invoke();
            return bulletObject;
        }
    }
}