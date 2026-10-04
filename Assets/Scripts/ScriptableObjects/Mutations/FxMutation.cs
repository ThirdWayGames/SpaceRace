using Assets.Scripts.Components;
using System;
using UnityEngine;

[Serializable]
public abstract class FxMutation<T> : RepeatableMutation where T : ScriptableObject
{
  
}