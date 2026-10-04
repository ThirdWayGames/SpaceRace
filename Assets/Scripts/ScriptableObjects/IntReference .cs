using System;

namespace Assets.Scripts.ScriptableObjects
{
    [Serializable]
    public class IntReference
    {
        public bool UseConstant = false;
        public int ConstantValue;
        public IntVariable Variable;

        public int Value
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
                    UnityEngine.Debug.LogWarning("Attempted to set the value of FloatReference when it was set to use the constant value.");
                }
            }
        }
    }
}