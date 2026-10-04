using System;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
public class DataCacheDifficultySettings : ScriptableObject
{
    public int SuccessAttempts = 3;

    public int StartingFailingSystemCount = 3;

    public float LockoutTime = 30;

    public float LockoutTimeMultiplier = 5;

    public bool RestrictMaxButtons = false;

    public int MaxButtonCount;
}
