using System.Collections.Generic;
using Assets.Scripts.Enums;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IWeaponary
    {
        /// <summary>
        /// Fires the specified player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="isRunning">if set to <c>true</c> [is running].</param>
        /// <param name="frameTiming">The frame timing.</param>
        /// <param name="isAIBullet">If the bullet is ai</param>
        /// <returns>The spawned bullet.</returns>
        GameObject Fire(IPlayerController player, bool isRunning, float frameTiming, bool isAIBullet = false);

        /// <summary>
        /// Alts the fire.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="isAltFire">if set to <c>true</c> [is alt fire].</param>
        /// <param name="isRunning">if set to <c>true</c> [is running].</param>
        /// <param name="frameTiming">The frame timing.</param>
        /// <returns>Game object relating to the alt firing process.</returns>
        GameObject AltFire(IPlayerController player, bool isAltFire, bool isRunning, float frameTiming);

        /// <summary>
        /// Gets the fire mode.
        /// </summary>
        /// <returns></returns>
        FireMode GetFireMode();

        /// <summary>
        /// Gets the fire rate.
        /// </summary>
        /// <returns></returns>
        float GetFireRate();

        /// <summary>
        /// Gets the available fire modes.
        /// </summary>
        /// <returns></returns>
        List<FireMode> GetAvailableFireModes();

        void Reload(int ammoAmount);

        int GetCurrentAmmo();

        int GetClipSize();

        /// <summary>
        /// Using the velocity and lifetime calculates the effective range.
        /// </summary>
        /// <returns>A float indicating the effective range.</returns>
        float GetEffectiveRange();

        /// <summary>
        /// Gets the min range the weapon will be used at.
        /// </summary>
        /// <returns>A float value indicating the min range at wich the weapon is used.</returns>
        float GetMinAttackRange();

        /// <summary>
        /// Plays the fire sound.
        /// </summary>
        /// <param name="player">The player.</param>
        void PlayFireSound();

        void PlayReloadSound();

        void PlayEmptyClipSound();

        /// <summary>
        /// Animates the specified player.
        /// </summary>
        /// <param name="player">The player.</param>
        void Animate(IPlayerController player);
    }
}