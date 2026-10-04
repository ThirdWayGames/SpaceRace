using System;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu]
    [Serializable]
    public class FloatVariable : ScriptableObject, IScriptableVariable
    {
        public float Value;

        public float DefaultVaule;
        public void Reset()
        {
            Value = DefaultVaule;
        }
    }
}
