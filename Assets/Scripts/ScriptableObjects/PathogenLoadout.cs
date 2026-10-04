using System;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects
{
    [CreateAssetMenu]
    [Serializable]
    public class PathogenLoadout : ScriptableObject
    {
        public string CurrentPathogen;

        public int CurrentPathogenSeverity;

        public void Awake()
        {
            CurrentPathogen = string.Empty;
            CurrentPathogenSeverity = 0;
        }
    }
}