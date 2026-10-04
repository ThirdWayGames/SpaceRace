using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects.Pathogens;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LexiconGameController : MonoBehaviour, IConsoleSceneController
{
    /// <summary>
    /// A list of LexiconEntries
    /// </summary>
    public ScriptableObject Lexicon;

    public GameObject LexiconEntryList;

    public GameObject LexiconEntryDetailImage;

    public GameObject LexiconEntryDetailName;

    public GameObject LexiconEntryDetailList;

    public GameObject LexiconEntryListPrefab;

    public GameObject LexiconEntryDetailPrefab;

    public ScriptableObject CurrentEntryItem;

    public LexiconEntryDetailController LexiconEntryDetailController;

    /// <summary>
    /// The current console that triggered the scene loading.
    /// </summary>
    private IConsole CurrentConsole { get; set; }

    /// <summary>
    /// Called when the user quits from the console.
    /// </summary>
    public void QuitConsole()
    {
        SceneManager.UnloadSceneAsync(this.gameObject.scene.name);
    }

    public void Start()
    {
        // Make sure the Lexicon is of the appropriate type of scriptable object.
        if (Lexicon == null || Lexicon as Lexicon == null)
        {
            Debug.LogWarning("Lexicon is null or not of type 'Lexicon'");
        }

        if (CurrentEntryItem == null)
        {
            LexiconEntryDetailImage.SetActive(false);
            LexiconEntryDetailName.SetActive(false);
        }
    }

    /// <summary>
    /// Clears the list of entry list items by destroying all the current children.
    /// </summary>
    public void ClearEntryList()
    {
        for (int i = 0; i < LexiconEntryList.transform.childCount; i ++)
        {
            Destroy(LexiconEntryList.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Loads all the current entries of a specified type into the list.
    /// </summary>
    /// <param name="lexiconEntryType"></param>
    public void LoadEntries(int lexiconEntryType)
    {
        var castLexicon = Lexicon as Lexicon;

        LexiconEntryDetailController.ClearCurrentEntryDetail();
        ClearEntryList();

        // Load the entries from the lexicon into the scroll view on the left.
        var entriesToList = castLexicon.GetEntries((LexiconEntryType)lexiconEntryType);

        foreach (var entryListItem in entriesToList.Cast<LexiconEntry>())
        {
            var entryItem = Instantiate(LexiconEntryListPrefab);
            var listEntryText = entryItem.GetComponent<LexiconListEntry>();
            if (listEntryText != null)
            {
                listEntryText.SetLexiconEntry(LexiconEntryDetailController, entryListItem);
            }

            entryItem.transform.SetParent(LexiconEntryList.transform, false);
        }

        var entryListItemToLoad = castLexicon.GetEntryListItem((LexiconEntryType)lexiconEntryType);
        if (LexiconEntryDetailController != null && entryListItemToLoad != null)
        {
            LexiconEntryDetailController.LoadEntryItem(entryListItemToLoad);
        }
    }

    /// <summary>
    /// Called from the console when the scene has loaded.
    /// </summary>
    /// <param name="currentConsole">The console that requested the scene be loaded.</param>
    public void InitaliseMiniGame(IConsole currentConsole)
    {
        CurrentConsole = currentConsole;
    }
}
