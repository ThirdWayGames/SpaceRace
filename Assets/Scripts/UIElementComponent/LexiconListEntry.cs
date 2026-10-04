using Assets.Scripts.ScriptableObjects.Pathogens;
using UnityEngine;
using UnityEngine.UI;

public class LexiconListEntry : MonoBehaviour
{
    public Text EntryName;

    public ScriptableObject EntryItem;

    protected LexiconEntryDetailController LexiconEntryDetailController;

    // Use this for initialization
    public void SetLexiconEntry (LexiconEntryDetailController lexiconEntryDetailController, ScriptableObject entryItem)
    {
        EntryItem = entryItem;
        LexiconEntryDetailController = lexiconEntryDetailController;

        UpdateDispaly();
    }

    public void LoadEntryDetail()
    {
        if (LexiconEntryDetailController != null && EntryItem != null && LexiconEntryDetailController.CurrentEntryItem != EntryItem)
        {
            LexiconEntryDetailController.LoadEntryItem(EntryItem);
        }
    }

    protected void UpdateDispaly()
    {
        var castEntryItem = EntryItem as LexiconEntry;
        if (castEntryItem != null)
        {
            EntryName.text = castEntryItem.Name;
        }
    }
}
