using Assets.Scripts.Enums;
using System;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Pathogens
{
    [CreateAssetMenu]
    [Serializable]
    public class PathogenTreatment : ScriptableObject
    {
        public int Difficulty;

        public PathogenTreatmentStrength Strength;

        public PathogenTreatmentType Type;
    }
}