using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyType : IOnCloneDeath
{
    [Header("Enemy Type Identifier")]
    public string Name;

    [Header("Enemy Type Object")]
    public GameObject Enemy;

    [Header("Enemy Type Settings")]
    public int EnemyCount;

    public float RateOfSpawn;

    public float TimeUntilNextEnemyType;

    [Header("Spawn Point. [Any, 'SpawnPointName']")]
    [SerializeField]
    public string SpawnPoint;

    public List<GameObject> EnemiesAlive { get; set; }

    public void OnCloneDeath(GameObject clone)
    {
        if (clone != null)
        {
            if (EnemiesAlive != null)
            {
                EnemiesAlive.Remove(clone);
            }
        }
    }
}
