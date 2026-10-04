using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Interfaces;
using UnityEngine;

// Iplayer controller until the shooting system is updated
public class EnemyAttackComponent : MonoBehaviour, IPlayerController
{
    public float CombatCooldown = 10f;

    public float LastAttackTime;

    public bool InCombat { get;  set; }

    /// <summary>
    /// Min distance at which I can attack the target.
    /// </summary>
    public float AttackDistance = 1.4f;

    public Transform FindChild(string gameObjectName)
    {
        throw new System.NotImplementedException();
    }

    public Transform GetTransform()
    {
        return transform;
    }

    public IWeaponary GetWeapon()
    {
        return GetComponentInChildren<IWeaponary>();
    }

    public void SetMaxSpeed(float maxSpeed)
    {
        throw new System.NotImplementedException();
    }

    public void SetDisableMovement(bool disabled)
    {
        throw new System.NotImplementedException();
    }

    public void SetMaxRunningSpeed(float maxRunningSpeeed)
    {
        throw new System.NotImplementedException();
    }

    public void ApplyDamage(float damage)
    {
        throw new System.NotImplementedException();
    }

    public Transform GetHandTransform(int handIndex)
    {
        throw new System.NotImplementedException();
    }

    public void InitialiseClientSpawn()
    {
        throw new System.NotImplementedException();
    }

    public void SetEquipment(IEquipment equipment)
    {
        throw new System.NotImplementedException();
    }

    public IEquipment GetEquipment()
    {
        throw new System.NotImplementedException();
    }

    public Animator UpdateVfxObject(int? state = null)
    {
        throw new System.NotImplementedException();
    }

    public void SetDisableActions(bool disabled)
    {
        throw new System.NotImplementedException();
    }

    public bool IsActionsDisabled()
    {
        return false;
    }

    public ScriptableObject GetPlayer()
    {
        return null;
    }

    public int GetTeamId()
    {
        var result = 0;
        var teamComp = GetComponent<TeamComponent>();
        if (teamComp != null)
        {
            result = teamComp.TeamIdentifier;
        }

        return result;
    }

    public void SetPathogenLoadout(ScriptableObject pathogenLoadout)
    {
    }
}
