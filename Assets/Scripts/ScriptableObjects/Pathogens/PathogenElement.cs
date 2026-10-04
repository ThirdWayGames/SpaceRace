using System;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Pathogens
{
    [CreateAssetMenu(menuName ="PathogenElement")]
    [Serializable]
    public class PathogenElement : ScriptableObject
    {
        public Color Color;

        public Color TextColor = new Color(255, 255, 255, 255);

        public Sprite Shape;

        public string Type;

        public string FriendlyName;

        public float Strength;

        public string GetName()
        {
            return !string.IsNullOrWhiteSpace(FriendlyName) ? FriendlyName : Type.ToString();
        }
    }
}