using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Pathogens
{
    [CreateAssetMenu]
    [Serializable]
    public class Pathogen : ScriptableObject
    {
        public string PathogenName;

        public string PathogenIcdCode;

        public int Severity;

        public PathogenInfectionType DeliveryType;

        [Header("Synthesis Elements")]
        public List<ScriptableObject> PathogenElements;

        [Header("Cure Elements")]
        public List<ScriptableObject> AntidotalElements;

        [Header("Symptoms")]
        public List<ScriptableObject> SymptomMutations;
    }
}