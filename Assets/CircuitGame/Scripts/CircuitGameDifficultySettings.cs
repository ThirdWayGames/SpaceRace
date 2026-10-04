using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
[Serializable]
public class CircuitGameDifficultySettings : ScriptableObject
{
    public int TimeAllowed = 30;

    public float PlayerSpeed = 15;

    public int AllowedAmountOfTries = 1;
}
