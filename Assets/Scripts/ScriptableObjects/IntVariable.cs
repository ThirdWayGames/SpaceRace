using System;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu]
    [Serializable]
    public class IntVariable : ScriptableObject, IScriptableVariable
    {
        public int Value;

        public int DefaultVaule;
        public void Reset()
        {
            Value = DefaultVaule;
        }
    }
}