using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Pathogens
{
    [CreateAssetMenu]
    [Serializable]
    public class LexiconEntry : ScriptableObject
    {
        /// <summary>
        /// The type of entry in the lexicon (Bacteria, Virus, Treatment)
        /// </summary>
        public LexiconEntryType EntryType;

        /// <summary>
        /// An image associated to the entry.
        /// </summary>
        public Sprite Image;

        public bool SetImageAsBackground;

        /// <summary>
        /// Used to tint the image if required.
        /// </summary>
        public bool ApplyImageTint;

        /// <summary>
        ///  If the image is due to be colourised then this tint can be ued to 
        /// </summary>
        public Color ImageTint;

        /// <summary>
        ///  The name of the entry.
        /// </summary>
        public string Name;

        /// <summary>
        /// A list of sub details for the entry.
        /// </summary>
        public List<ScriptableObject> EntrySubDetails;
    }
}