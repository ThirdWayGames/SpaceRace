using System;
using Assets.Scripts.Interfaces;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [Serializable]
    public class VariableReseterReference: IVariableReference
    {
        public ScriptableObject VariableSetter;

        public virtual void Reset()
        {
            ((IScriptableVariable)VariableSetter).Reset();
        }
    }
}