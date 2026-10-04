using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.ScriptableObjects.Pathogens
{
    [CreateAssetMenu]
    [Serializable]
    public class LexiconEntryDetailItem : ScriptableObject
    {
        public ScriptableObject ImageLeft;
        public string Title;
        public string Description;
        public ScriptableObject ImageRight;
    }
}