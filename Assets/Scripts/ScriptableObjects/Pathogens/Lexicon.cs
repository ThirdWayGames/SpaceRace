using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.ScriptableObjects.Pathogens
{
    [CreateAssetMenu]
    [Serializable]
    public class Lexicon : ScriptableObject
    {
        /// <summary>
        /// A list of entries of bactira type.
        /// </summary>
        public List<ScriptableObject> BacteriaEntries;

        /// <summary>
        /// The List Entry Item that is loaded when the tab is selected.
        /// </summary>
        public ScriptableObject BacteriaListEntry;

        /// <summary>
        /// A list of entries of virus type
        /// </summary>
        public List<ScriptableObject> ViralEntries;

        /// <summary>
        /// The List Entry Item that is loaded when the tab is selected.
        /// </summary>
        public ScriptableObject ViralListEntry;

        /// <summary>
        /// A list of entries of treatment type.
        /// </summary>
        public List<ScriptableObject> TreatmentEntries;

        /// <summary>
        /// The List Entry Item that is loaded when the tab is selected.
        /// </summary>
        public ScriptableObject TreatmentListEntry;

        public void Start()
        {
            // Perform a check to make sure that all the entries in a list are of the approriate type.
            if (BacteriaEntries != null && !BacteriaEntries.Cast<LexiconEntry>().All(x => x.EntryType == LexiconEntryType.Bacteria))
            {
                Debug.LogWarning("Non bacterial entries found in the bactria entries in the lexicon.");
            }

            if (ViralEntries != null && !ViralEntries.Cast<LexiconEntry>().All(x => x.EntryType == LexiconEntryType.Virus))
            {
                Debug.LogWarning("Non viral entries found in the viral entries in the lexicon.");
            }

            if (TreatmentEntries != null && !TreatmentEntries.Cast<LexiconEntry>().All(x => x.EntryType == LexiconEntryType.Treatment))
            {
                Debug.LogWarning("Non treatment entries found in the treatment entries in the lexicon.");
            }
        }

        /// <summary>
        /// Gets the appropriate list of entries for the defined type.
        /// </summary>
        /// <param name="lexiconEntryType">The type of lexicon entries we are looking for.</param>
        /// <returns>The list of lexicon entries of the defined type.</returns>
        public List<ScriptableObject> GetEntries(LexiconEntryType lexiconEntryType)
        {
            List<ScriptableObject> result = null;
            switch(lexiconEntryType)
            {
                case LexiconEntryType.Bacteria:
                    {
                        result = BacteriaEntries;
                        break;
                    }
                case LexiconEntryType.Virus:
                    {
                        result = ViralEntries;
                        break;
                    }
                case LexiconEntryType.Treatment:
                    {
                        result = TreatmentEntries;
                        break;
                    }
            }

            return result;
        }

        public ScriptableObject GetEntryListItem(LexiconEntryType lexiconEntryType)
        {
            ScriptableObject result = null;
            switch (lexiconEntryType)
            {
                case LexiconEntryType.Bacteria:
                    {
                        result = BacteriaListEntry;
                        break;
                    }
                case LexiconEntryType.Virus:
                    {
                        result = ViralListEntry;
                        break;
                    }
                case LexiconEntryType.Treatment:
                    {
                        result = TreatmentListEntry;
                        break;
                    }
            }

            return result;
        }
    }
}