using System;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu]
    [Serializable]
    public class BooleanVariable : ScriptableObject, IScriptableVariable
    {
        public bool Value;

        public bool DefaultVaule;

        public void Reset()
        {
            Value = DefaultVaule;
        }
    }
}