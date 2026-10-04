using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;

[Serializable]
public class Wave
{
    [Header("Wave Identifier")]
    public string Name;

    [Header("Enemy Type Management")]
    public EnemyType[] EnemyTypes;

    [Header("Wave Settings")]
    public bool EliminateWaveToAdvance;
    public float WaveTimeout;

    public bool FinishedSpawning { get; set; }

    public bool WaveComplete { get; set; }
}
