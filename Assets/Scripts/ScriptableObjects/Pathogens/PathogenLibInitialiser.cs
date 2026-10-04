using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Pathogens
{
    [CreateAssetMenu]
    [Serializable]
    public class PathogenLibInitialiser : ScriptableObject
    {
        public int TeamId;

        public List<ScriptableObject> CurablePathogens;

        public List<ScriptableObject> KnownPathogens;
    }
}