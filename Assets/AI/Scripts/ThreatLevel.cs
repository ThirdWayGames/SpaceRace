using Assets.Scripts.Interfaces;
using System;
using UnityEngine;

[Serializable]
public class ThreatLevel
{
    public GameObject Target;
    public bool IsHostile;
    public bool IsInLos;
    public bool IsInShotLos;
    public int ThreatScore;
    public string ThreatScoreDesc;
    public float Distance;
    public float TimeLastSeen;

    /// <summary>
    /// Determines if the target can actually be attacked based its visibility and shot LOS (Not distance).
    /// </summary>
    /// <returns>Returns true if the target can be attacked.</returns>
    public bool CanAttack(IWeaponary weapon)
    {
        return IsHostile && IsInLos && IsInShotLos && (Distance <= weapon.GetEffectiveRange());
    }
}
