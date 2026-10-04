using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatsComponent : MonoBehaviour {


    public float maxHealth = 100;
    public float curHealth { get; set; }
    public Stat damage;
    public Stat armor;

    public float damageToSubtract { get; set; }

    public bool isTakingDamage;

    void Start()
    {
        curHealth = maxHealth;
    }

    public event Action OnDeathEvent;

    public void InvokeOnDeathEvent()
    {
        if (OnDeathEvent != null)
        {
            OnDeathEvent.Invoke();
        }
    }


}
