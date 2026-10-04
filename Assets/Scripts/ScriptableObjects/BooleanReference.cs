using System;

namespace Assets.Scripts.ScriptableObjects
{
    [Serializable]
    public class BooleanReference
    {
        public bool UseConstant = false;
        public bool ConstantValue;
        public BooleanVariable Variable;

        public bool Value
        {
            get
            {
                return !UseConstant && Variable != null ? Variable.Value : ConstantValue;
            }
            set
            {
                if (!UseConstant && Variable != null)
                {
                    Variable.Value = value;
                }
                else
                {
                    UnityEngine.Debug.LogWarning("Attempted to set the value of BooleanReference when it was set to use the constant value.");
                }
            }
        }
    }
}