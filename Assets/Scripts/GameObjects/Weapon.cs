using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using Assets.Scripts.Components;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.GameObjects
{
    [RequireComponent(typeof(AudioSource))]
    public abstract class Weapon : Equipment, IWeaponary, IDisposeCallback
    {
        public float FireDelay = 0.9f;
        public float MinAttackDistance = 1f;
        public float MinBulletDeviation = 0.5f;
        public float MaxBulletDeviation = 1.5f;
        public float BulletVelocity = 45f;
        public float BulletLifetime = 0.5f;
        public float EneryConsuption = 10;
        public bool AimAtMouseCentre;
        public int ClipSize = 0;
        public float ReloadTimer = 0;

        public GameObject BulletPrefab = null;
        public GameObject CloudBulletPrefab = null;
        public GameObject SprayPrefab = null;

        public AudioClip FireEffect;
        public AudioClip EmptyClipEffect;
        public AudioClip ReloadEffect;

        protected bool Reloading = false;
        protected float ReloadingTime = 0;
        protected int CurrentAmmo = 0;
        protected int ReloadAmount = 0;
        protected GameObject CurrentBeam;

        /// <summary>
        /// The available fire modes
        /// </summary>
        public List<FireMode> AvailableFireModes;

        /// <summary>
        /// The current fire mode
        /// </summary>
        public FireMode CurrentFireMode;

        /// <summary>
        /// The fire timer
        /// </summary>
        protected float FireTimer;

        /// <summary>
        /// The running
        /// </summary>
        protected bool Running;

        /// <summary>
        /// Initializes a new instance of the <see cref="Weapon"/> class.
        /// </summary>
        public virtual void Awake()
        {
            FireTimer = FireDelay;
            AvailableFireModes = new List<FireMode>();
            CurrentFireMode = AvailableFireModes.FirstOrDefault();
            CurrentAmmo = ClipSize;
        }

        public UnityEvent OnFireEvent;

        public bool IsAIBullet = false;

        public void Update()
        {
            FireTimer += Time.deltaTime;

            if (Reloading)
            {
                ReloadingTime += Time.deltaTime;

                if (ReloadingTime >= ReloadTimer)
                {
                    CurrentAmmo = ReloadAmount;
                    ReloadingTime = 0;
                    Reloading = false;
                }
            }
        }

        public virtual void CycleFireMode()
        {
            if (CurrentFireMode == FireMode.Single)
            {
                if (SprayPrefab != null)
                {
                    CurrentFireMode = FireMode.Beam;
                }

                if (CloudBulletPrefab != null)
                {
                    CurrentFireMode = FireMode.Cloud;
                }
            }
            else if (CurrentFireMode == FireMode.Cloud)
            {
                if (BulletPrefab != null)
                {
                    CurrentFireMode = FireMode.Single;
                }

                if (SprayPrefab != null)
                {
                    CurrentFireMode = FireMode.Beam;
                }
            }
            else
            {
                if (CloudBulletPrefab != null)
                {
                    CurrentFireMode = FireMode.Cloud;
                }

                if (BulletPrefab != null)
                {
                    CurrentFireMode = FireMode.Single;
                }
            }
        }

        /// <summary>
        /// Gets the fire mode.
        /// </summary>
        /// <returns></returns>
        public virtual FireMode GetFireMode()
        {
            return CurrentFireMode;
        }

        /// <summary>
        /// Gets the available fire modes.
        /// </summary>
        /// <returns>The list of available fire modes for the weapon</returns>
        public virtual List<FireMode> GetAvailableFireModes()
        {
            return AvailableFireModes;
        }

        /// <summary>
        /// Gets the fire rate.
        /// </summary>
        /// <returns></returns>
        public virtual float GetFireRate()
        {
            return FireDelay;
        }

        /// <summary>
        /// Thrown weapons build power while the fire button is held, then release on button up.
        /// </summary>
        public virtual bool ChargesThrow
        {
            get { return false; }
        }

        public virtual bool OccupiesBothHands
        {
            get { return false; }
        }

        public virtual float ThrowChargeSeconds
        {
            get { return 0f; }
        }

        public virtual float ThrowSpeed
        {
            get { return BulletVelocity; }
        }

        public virtual bool IsThrowCharging
        {
            get { return false; }
        }

        public virtual void BeginThrowCharge()
        {
        }

        public virtual void AccumulateThrowCharge(float deltaTime)
        {
        }

        public virtual void ClearThrowCharge()
        {
        }

        /// <summary>
        /// A charged throw leaves the hand when the fire button is released, including a short tap.
        /// </summary>
        public virtual GameObject ReleaseChargedThrow(IPlayerController player, bool isRunning)
        {
            if (FireTimer < FireDelay)
            {
                FireTimer = FireDelay;
            }

            return Fire(player, isRunning, Time.deltaTime);
        }

        /// <summary>
        /// Fires the weapon the player has equiped.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="isRunning">if set to <c>true</c> [is running].</param>
        /// <param name="frameTiming">The frame timing.</param>
        /// <param name="isAIBullet">If the bullet is ai</param>
        /// <returns>The spawned bullet.</returns>
        public virtual GameObject Fire(IPlayerController player, bool isRunning, float frameTiming, bool isAIBullet = false)
        {
            GameObject spawnedBullet = null;

            // Set the IsAIBullet variable
            IsAIBullet = isAIBullet;

            if (player != null && !player.IsActionsDisabled())
            {
                Running = isRunning;
                if (FireTimer >= FireDelay)
                {
                    if (HasEnergy(player))
                    {
                        // reset the fire timer.
                        FireTimer = 0f;

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
                            PhotonNetwork.RPC(photonView, "PlayFireSound", PhotonNetworkSettings.EventTarget, false, null);
                        }
                        else
                        {
                            PlayFireSound();
                        }

                        // Fire the gun.
                        spawnedBullet = SpawnBullet(player, gunPort.position, Quaternion.Euler(rot));

                        // Reduce ammo by 1;
                        ConsumeEnergy(player);
                        OnFireEvent.Invoke();
                    }
                    else
                    {
                        // Play sound
                        if (PhotonNetwork.inRoom && photonView != null)
                        {
                            PhotonNetwork.RPC(photonView, "PlayEmptyClipSound", PhotonNetworkSettings.EventTarget, false, null);
                        }
                        else
                        {
                            PlayEmptyClipSound();
                        }
                    }
                }
            }

            return spawnedBullet;
        }

        public virtual GameObject FireBeam(IPlayerController player, bool isRunning, float frameTiming, bool isActive = false)
        {
            if (player != null && !player.IsActionsDisabled())
            {
                // If we are firing the beam and there is not one already.
                if (isActive)
                {
                    var gunPort = this.gameObject.transform.Find("Muzzle");
                    if (gunPort == null)
                    {
                        Debug.LogError("Can't find 'Muzzle' object to spawn bullet at.");
                        return null;
                    }

                    Running = isRunning;
                    if (HasEnergy(player))
                    {
                        // Fire the gun.
                        if (gunPort.transform.childCount <= 0)
                        {
                            // Play sound
                            if (PhotonNetwork.inRoom && photonView != null)
                            {
                                PhotonNetwork.RPC(photonView, "PlayFireSound", PhotonNetworkSettings.EventTarget, false, null);
                            }
                            else
                            {
                                PlayFireSound();
                            }

                            if (CurrentBeam == null)
                            {
                                var rot = ApplyBulletSpread(player, gunPort);
                                CurrentBeam = SpawnBullet(player, gunPort.position, Quaternion.Euler(rot));
                            }

                            AimBeamAtCursor(gunPort);
                        }

                        // Reduce ammo by 1;
                        ConsumeEnergy(player);
                        OnFireEvent.Invoke();
                    }
                    else
                    {
                        // Despawn the beam if we run out of energy.
                        DespawnBullet();

                        // Play sound
                        if (PhotonNetwork.inRoom && photonView != null)
                        {
                            PhotonNetwork.RPC(photonView, "PlayEmptyClipSound", PhotonNetworkSettings.EventTarget, false, null);
                        }
                        else
                        {
                            PlayEmptyClipSound();
                        }
                    }
                } else
                {
                    DespawnBullet();
                }
            }

            return CurrentBeam;
        }

        /// <summary>
        /// Contains logic that is applied to the player when they are alt firing.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="isAltFire">if set to <c>true</c> [is alt fire].</param>
        /// <param name="isRunning">if set to <c>true</c> [is running].</param>
        /// <param name="frameTiming">The frame timing.</param>
        /// <returns>
        /// Game object relating to the alt firing process.
        /// </returns>
        public virtual GameObject AltFire(IPlayerController player, bool isAltFire, bool isRunning, float frameTiming)
        {
            return null;
        }

        public virtual void Reload(int reloadAmount)
        {
            if (!Reloading)
            {
                Reloading = true;

                // Play sound
                if (PhotonNetwork.inRoom && photonView != null)
                {
                    PhotonNetwork.RPC(photonView, "PlayReloadSound", PhotonNetworkSettings.EventTarget, false, null);
                }
                else
                {
                    PlayReloadSound();
                }
                
                ReloadAmount = reloadAmount;
            }
        }

        public int GetCurrentAmmo()
        {
            return CurrentAmmo;
        }

        public int GetClipSize()
        {
            return ClipSize;
        }

        /// <summary>
        /// Plays the fire sound.
        /// </summary>
        [PunRPC]
        public virtual void PlayFireSound()
        {
            if (Reloading)
            {
                return;
            }

            var audioSource = GetAudioPlayer();
            if (audioSource != null && FireEffect != null)
            {
                audioSource.PlayOneShot(FireEffect);
            }

            var muzzle = transform.Find("Muzzle");
            if (muzzle != null)
            {
                ShotEffects.Flash(muzzle.position, muzzle.forward, gameObject.name);
            }
        }

        [PunRPC]
        public virtual void PlayReloadSound()
        {
            var audioSource = GetAudioPlayer();
            if (audioSource != null && ReloadEffect != null)
            {
                audioSource.PlayOneShot(ReloadEffect);
            }
        }

        [PunRPC]
        public virtual void PlayEmptyClipSound()
        {
            if (!Reloading)
            {
                var audioSource = GetAudioPlayer();
                if (audioSource != null && EmptyClipEffect != null)
                {
                    audioSource.PlayOneShot(EmptyClipEffect);
                }
            }
        }

        /// <summary>
        /// Animates this instance.
        /// </summary>
        public virtual void Animate(IPlayerController player)
        {
            Debug.Log(string.Format("No animation defined for '{0}'. Please override Animate() method.", transform.name));
        }

        public float GetMinAttackRange()
        {
            return MinAttackDistance;
        }

        public float GetEffectiveRange()
        {
            var effectiveRange = BulletVelocity * BulletLifetime;
            return effectiveRange > 0 ? effectiveRange : MinAttackDistance;
        }

        /// <summary>
        /// Applies the bullet spread.
        /// </summary>
        /// <param name="gunPortPos">The spawn position.</param>
        /// <returns>
        /// The spawn rotation for the bullet, deviated ramdomly between Min and Max bullet deviation.
        /// </returns>
        protected virtual void AimBeamAtCursor(Transform muzzle)
        {
            if (CurrentBeam == null || muzzle == null || IsAIBullet)
            {
                return;
            }

            var fallback = muzzle.forward;
            var direction = ShotAim.Direction(muzzle.position, ShotAim.CursorPoint(muzzle.position, fallback), fallback);
            CurrentBeam.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        protected virtual Vector3 ApplyBulletSpread(IPlayerController player, Transform gunPortPos)
        {
            // Calcuate the current deviation based amount of focus and the min and max deviation.
            var currentDeviation = Random.Range(MinBulletDeviation, MaxBulletDeviation);
            var focusComponent = player.GetComponent<StaminaComponent>();
            if (focusComponent != null)
            {
                // If my focus is at full and I am standing still
                if (focusComponent.CurrentValue == 100 && player.GetTransform().GetComponent<Rigidbody>().velocity != Vector3.zero)
                {
                    // Set the deviation of the gun to be zero.
                    currentDeviation = 0;
                } else
                {
                    // Calculate the deviation based on my current focus value.
                    var deviationDelta = MaxBulletDeviation - MinBulletDeviation;
                    currentDeviation = MaxBulletDeviation - (deviationDelta * (focusComponent.CurrentValue / 100));
                }
            }

            Quaternion aimRotation;
            if (!IsAIBullet)
            {
                var fallback = gunPortPos.forward;
                var cursor = ShotAim.CursorPoint(gunPortPos.position, fallback);
                var direction = ShotAim.Direction(gunPortPos.position, cursor, fallback);
                aimRotation = Quaternion.LookRotation(direction, Vector3.up);
            }
            else
            {
                aimRotation = gunPortPos.rotation;
            }

            var spread = Quaternion.Euler(
                Random.Range(-currentDeviation, currentDeviation),
                Random.Range(-currentDeviation, currentDeviation),
                0f);
            return (aimRotation * spread).eulerAngles;
        }

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
        protected virtual GameObject SpawnBullet(IPlayerController player, Vector3 position, Quaternion rotation)
        {
            GameObject spawnedBullet = null;
            var selectedBulletToSpawn = GetBulletToSpawn();
            if (selectedBulletToSpawn != null)
            {
                var data = GenerateSpawnData(player);
                var bulletData = data as BulletData;
                if (bulletData != null)
                {
                    var muzzle = transform.Find("Muzzle");
                    var shotForward = rotation * Vector3.forward;
                    var muzzleForward = muzzle != null ? muzzle.forward : shotForward;
                    bulletData.AimYaw = ShotAim.YawOffset(muzzleForward, shotForward);
                }

                var array = data.ToOjectArray();

                // If we are in a network game.
                if (PhotonNetwork.inRoom)
                {
                    // Network spawn the bullet with the velocity/lifetime data.
                    spawnedBullet = PhotonNetwork.Instantiate(selectedBulletToSpawn.name, position, rotation, 0, array);
                }
                else
                {
                    // Get the team id from the person firing the gun (assume 0 [AI])
                    var teamComponent = player.GetComponent<TeamComponent>();
                    var teamId = teamComponent != null ? teamComponent.TeamIdentifier : 0;

                    // If we are not in a network game.
                    spawnedBullet = Instantiate(selectedBulletToSpawn, position, rotation);
                    spawnedBullet.GetComponent<IBullet>().SetSpawnData(data);
                    /*var bulletComp = spawnedBullet.GetComponent<IBullet>();
                    if (bulletComp != null)
                    {
                        bulletComp.ApplyForce(BulletVelocity, BulletLifetime);
                        bulletComp.SetShooterTeamId(teamId);
                    }

                    // If we have a local attach to parent.
                    var localAttachToParent = spawnedBullet.GetComponent<IAttachToParent>();
                    if (localAttachToParent != null)
                    {
                        // Instantiate it.
                        localAttachToParent.Instantiate(GenerateSpawnData(player).ToOjectArray());
                    }*/
                }
            }

            return spawnedBullet;
        }

        protected virtual GameObject GetBulletToSpawn()
        {
            // Determine which bullet prefab to use.
            GameObject selectedFireModePrefab = null;

            // Get the name of the prefab based on the current fire mode.
            if (CurrentFireMode == Enums.FireMode.Cloud)
            {
                selectedFireModePrefab = CloudBulletPrefab;
            }
            else if (CurrentFireMode == Enums.FireMode.Beam)
            {
                selectedFireModePrefab = SprayPrefab;
            }
            else
            {
                selectedFireModePrefab = BulletPrefab;
            }

            if (selectedFireModePrefab == null)
            {
                Debug.LogError(string.Format("No prefab assigned to weapon: '{0}' for mode '{1}'", ItemName(), CurrentFireMode));
            }

            return selectedFireModePrefab;
        }

        protected virtual void DespawnBullet()
        {
            if (CurrentBeam != null)
            {
                // Decouple the beam from the parent
                CurrentBeam.gameObject.transform.SetParent(null);

                if (CurrentBeam != null)
                {
                    var beam = CurrentBeam.GetComponent<BeamBullet>();
                    if (beam != null)
                    {
                        if (beam.part.isPlaying)
                        {
                            beam.part.Stop();
                        }
                        else
                        {
                            if (!beam.part.isEmitting)
                            {
                                // If we are in a network game.
                                if (PhotonNetwork.inRoom)
                                {
                                    // Network spawn the bullet with the velocity/lifetime data.
                                    PhotonNetwork.Destroy(CurrentBeam.gameObject);
                                }
                                else
                                {
                                    Destroy(CurrentBeam.gameObject);
                                }
                            }
                        }
                    }
                    else
                    {
                        // If we are in a network game.
                        if (PhotonNetwork.inRoom)
                        {
                            // Network spawn the bullet with the velocity/lifetime data.
                            PhotonNetwork.Destroy(CurrentBeam.gameObject);
                        }
                        else
                        {
                            Destroy(CurrentBeam.gameObject);
                        }
                    }
                }
            }
        }

        protected virtual bool HasEnergy(IPlayerController player)
        {
            var result = false;
            var energyComponent = player.GetTransform().gameObject.GetComponentInParent<EnergyComponent>();
            if (energyComponent == null)
            {
                energyComponent = player.GetComponent<EnergyComponent>();
                if (energyComponent == null)
                {
                    Debug.LogWarning(string.Format("No energy component on '{0}'", this.name));
                    return result;
                }

            }
            return energyComponent.CurrentValue - GetEnergyToConsume() > 0;
        }

        protected virtual void ConsumeEnergy(IPlayerController player)
        {
            var energyComponent = player.GetTransform().gameObject.GetComponentInParent<EnergyComponent>();
            if (energyComponent == null)
            {
                Debug.LogWarning(string.Format("No energy component on '{0}'", this.name));
                return;
            }

            energyComponent.CurrentValue = Mathf.Clamp(energyComponent.CurrentValue -= GetEnergyToConsume(), 0, energyComponent.MaxValue);
        }

        protected float GetEnergyToConsume()
        {
            var energyToUse = EneryConsuption;
            var spawnedBullet = GetBulletToSpawn();
            if (spawnedBullet != null)
            {
                var bulletComp = spawnedBullet.GetComponent<IBullet>();
                var bulletEnergy = 0f;
                if (bulletComp != null)
                {
                    bulletEnergy = bulletComp.GetEnergyConsumption();
                }

                energyToUse = bulletEnergy > -1f ? bulletEnergy : energyToUse;
            }
            else
            {
                energyToUse = 0;
            }

            return energyToUse;
        }

        protected AudioSource GetAudioPlayer()
        {
            var afxObject = this.transform.Find("AFX");
            if (afxObject == null)
            {
                Debug.LogError(string.Format("No AFX game object found on'{0}'", transform.name));
                return null;
            }

            var audioSource = afxObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError(string.Format("No Audio source found on'{0}'", transform.name));
                return null;
            }

            return audioSource;
        }

        protected virtual ISpawnData GenerateSpawnData(IPlayerController player)
        {
            // Get the team id from the person firing the gun (assume 0 [AI])
            var teamComponent = player.GetComponent<TeamComponent>();
            var teamId = teamComponent != null ? teamComponent.TeamIdentifier : 0;

            var gunPort = this.gameObject.transform.Find("Muzzle");
            int? parentId = null;
            if (gunPort != null)
            {
                if (PhotonNetwork.inRoom)
                {
                    var photonView = gunPort.GetComponentInParent<PhotonView>();
                    if (photonView != null)
                    {
                        parentId = photonView.viewID;
                    }
                }
                else
                {
                    parentId = gunPort.transform.GetInstanceID();
                }
            }

            return new BulletData {
                BulletVelocity = BulletVelocity,
                BulletLifetime = BulletLifetime,
                ShooterId = teamId,
                ParentId = parentId,
                SubParentName = "Muzzle"
            };
        }

        public void DisposeItem(float currentTime)
        {
            // If we have a current beam
            if (CurrentBeam != null)
            {
                // Destroy it as we are being disposed of.
                Destroy(CurrentBeam);
            }
        }
    }
}