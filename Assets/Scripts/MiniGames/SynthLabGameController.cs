using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects.Pathogens;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SynthLabGameController : MonoBehaviour, IConsoleSceneController
{
    public List<GameObject> Sockets;

    public List<ScriptableObject> PathogenElements;

    public GameObject ColourGuessPrefab;
        
    public GameObject PathogenElementPrefab;

    public GameObject ElementStore;

    public GameObject ColourGuessPanel;

    public GameObject ColourGuessItemList;

    public GameObject ColourResultPanelLeft;

    public GameObject ColourResultPanelRight;

    public List<ScriptableObject> Pathogens;

    public Pathogen CurrentPathogenInSynthesis = null;

    public Text PathogenSynthesised;

    public Button TestButton;

    public Button ResetButton;

    public GameObject GameCompletePanel;

    public Text PathogenWinText;

    public Text ExecuteGuessErrorText;

    private List<string> CurrentTestItems;

    private List<GameObject> CurrentItemPickers;

    private int CurrentGuessCount = 0;

    private IConsole CurrentConsole { get; set; }

    public void Awake()
    {
        // For each pathogen element 
        if (PathogenElements != null && PathogenElements.Any())
        {
            foreach (var pathogenElement in PathogenElements.Cast<PathogenElement>())
            {
                // Spawn a pathogen element and add it to the pathogen element store
                var elementDisplay = PathogenElementPrefab.GetComponent<PathogenElementDisplay>();
                elementDisplay.PathogenElement = pathogenElement;
                var element = GameObject.Instantiate(PathogenElementPrefab);
                element.transform.SetParent(ElementStore.transform);
            }
        }

        if (GameCompletePanel != null) GameCompletePanel.SetActive(false);
    }

    public void Start()
    {
        ResetMiniGame();
    }

    public void Update()
    {
        if (TestButton != null) TestButton.interactable = Sockets.All(x => x.transform.childCount > 0) && CurrentPathogenInSynthesis == null;
        if (ResetButton != null) ResetButton.interactable = Sockets.Any(x => x.transform.childCount > 0) || CurrentPathogenInSynthesis != null;

        // Iterate the sockects
        foreach (var item in Sockets)
        {
            // Lock them if a viable pathogen has been found
            var dropController = item.GetComponent<PathogenElementDropHandler>();
            if (dropController != null)
            {
                dropController.SocketLocked = CurrentPathogenInSynthesis != null;
            }
        }
    }

    public void QuitConsole()
    {
        SceneManager.UnloadSceneAsync(this.gameObject.scene.name);
    }

    public void InitaliseMiniGame(IConsole currentConsole)
    {
        CurrentConsole = currentConsole;
        CurrentGuessCount = 0;
        ResetMiniGame();
    }

    public void ResetMiniGame()
    {
        CurrentGuessCount = 0;
        PathogenSynthesised.text = "";
        CurrentPathogenInSynthesis = null;
        if (ColourGuessPanel != null) ColourGuessPanel.gameObject.SetActive(false);
        if (TestButton != null) TestButton.interactable = false;
        if (ResetButton != null) ResetButton.interactable = false;
        CurrentTestItems = new List<string>();
        CurrentItemPickers = new List<GameObject>();
        ClearSockets();
        ClearColourResults();
    }

    public void ClearSockets()
    {
        foreach (var socket in Sockets)
        {
            if (socket.transform.childCount > 0)
            {
                for (int i = 0; i < socket.transform.childCount; i++)
                {
                    Destroy(socket.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public void SynthesiseSockets()
    {
        CurrentGuessCount = 0;
        if (Sockets.All(x => x.transform.childCount > 0))
        {
            foreach (var pathogen in Pathogens.Cast<Pathogen>())
            {
                var pathElementList = pathogen.PathogenElements.Cast<PathogenElement>();
                var socketElements = Sockets.Where(x => x.transform.childCount == 1).Select(x => x.transform.GetChild(0).GetComponent<PathogenElementDisplay>().PathogenElement).Cast<PathogenElement>().ToList();
                var pathMatch = 0;
                foreach (var pathElement in pathElementList)
                {
                    var matchedSocket = socketElements.FirstOrDefault(x => x.Color == pathElement.Color && x.Shape == pathElement.Shape);
                    if (matchedSocket != null)
                    {
                        // Mark the socket as having been matched.
                        pathMatch++;

                        // Remove the socket from further matches.
                        socketElements.Remove(matchedSocket);
                    }
                }

                // If the number of matches equals the number of elements.
                if (pathElementList.Count() == pathMatch)
                {
                    // Then we have a viable pathogen.
                    CurrentPathogenInSynthesis = pathogen;

                    // Break out, no futher tests on other pathogens.
                    break;
                }
            }

            /// Determine the output based on if we have a current pathogen in synthesis.
            if (CurrentPathogenInSynthesis != null)
            {
                PathogenSynthesised.text = "VIABLE PATHOGEN FOUND";
                InitialiseSynthMiniGame(CurrentPathogenInSynthesis);
            }
            else
            {
                PathogenSynthesised.text = "NO PATHOGEN FOUND";
                InitialiseSynthMiniGame(null);
            }
        }
        else
        {
            PathogenSynthesised.text = "All Sockets require elements;";
        }
    }

    public void ExecuteGuess()
    {
        var colourTestPassed = true;
        GameObject currentResultPanel;
        var resultItemIndex = 0;

        // Determine which result panel to use.
        currentResultPanel = CurrentGuessCount < 5 ? ColourResultPanelLeft : ColourResultPanelRight;
        resultItemIndex = CurrentGuessCount < 5 ? CurrentGuessCount : CurrentGuessCount - 5;

        if (CurrentItemPickers.Any() && CurrentItemPickers.All(x => !string.IsNullOrWhiteSpace(x.GetComponentInChildren<Text>().text)))
        {
            if (ExecuteGuessErrorText != null) ExecuteGuessErrorText.text = "";

            // If we have a result panel.
            if (currentResultPanel != null)
            {
                // Get the result item from the result panel based on the guess index.
                var itemResultItem = currentResultPanel.transform.GetChild(resultItemIndex);

                // Create new list of testable colours from the CurrentTestColours
                var itemTestList = new List<KeyValuePair<string, bool>>();
                foreach (var currentItem in CurrentTestItems)
                {
                    itemTestList.Add(new KeyValuePair<string, bool>(currentItem, false));
                }

                itemResultItem.GetComponent<LetterResult>().SetResults(ExtractSelectedItemFromPickers(CurrentItemPickers), CurrentTestItems);
                colourTestPassed = itemResultItem.GetComponent<LetterResult>().ProcessResult();

                itemResultItem.gameObject.SetActive(true);
                CurrentGuessCount++;
            }

            if (colourTestPassed)
            {
                // Add the pathogen to the list of available pathogens.
                var pathogenLib = FindObjectOfType<PathogenLibrary>();
                var playerManager = FindObjectOfType<PlayerManager3D>();
                if (pathogenLib != null && playerManager != null)
                {
                    var teamComp = playerManager.GetLocalPlayer().GetComponent<TeamComponent>();
                    if (teamComp != null)
                    {
                        pathogenLib.AddPathogenLocally(teamComp.TeamIdentifier, CurrentPathogenInSynthesis.name);
                    }
                    else
                    {
                        Debug.LogWarning("No teamp comp found on current local player.");
                    }
                }
                else
                {
                    Debug.LogWarning("No pathogen lib or player manager found in scene.");
                }

                if (PathogenWinText != null) PathogenWinText.text = string.Format("{0} now selectable for the Pathogen Blaster", CurrentPathogenInSynthesis.PathogenName);
                if (GameCompletePanel != null) GameCompletePanel.SetActive(true);

                if (CurrentConsole != null)
                {
                    CurrentConsole.SetConsoleResult(1);
                }
            }
        }
        else
        {
            if (ExecuteGuessErrorText != null) ExecuteGuessErrorText.text = "ERROR: ALL ITEMS MUST HAVE A VALUE";
        }
    }

    protected void ClearColourResults()
    {
        if (ColourResultPanelLeft != null)
        {
            // For each colour result in the left panel.
            for (int i = 0; i < ColourResultPanelLeft.transform.childCount; i++)
            {
                var colourResultItem = ColourResultPanelLeft.transform.GetChild(i);
                colourResultItem.gameObject.SetActive(false);
            }
        }

        if (ColourResultPanelRight != null)
        {
            // For each colour result in the left panel.
            for (int i = 0; i < ColourResultPanelRight.transform.childCount; i++)
            {
                var colourResultItem = ColourResultPanelRight.transform.GetChild(i);
                colourResultItem.gameObject.SetActive(false);
            }
        }
    }

    protected void InitialiseSynthMiniGame(Pathogen foundPathogen)
    {
        if (ColourGuessPanel != null)
        {
            // If a pathogen has been passed in make the ColourGuessPanel visible.
            ColourGuessPanel.gameObject.SetActive(foundPathogen != null);

            // Clear the test colours
            if (CurrentTestItems != null && CurrentTestItems.Any())
            {
                CurrentTestItems.Clear();
            }

            // Clear the colour picker GOs
            if (CurrentItemPickers != null && CurrentItemPickers.Any())
            {
                for (int i = 0; i < CurrentItemPickers.Count; i++)
                {
                    Destroy(CurrentItemPickers[i]);
                }

                CurrentItemPickers.Clear();
            }

            // if the Guess Item List is not null and has children
            if (ColourGuessItemList != null && ColourGuessItemList.transform.childCount > 0)
            {
                // Destroy the children.
                while (ColourGuessItemList.transform.childCount > 0)
                {
                    var child = ColourGuessItemList.transform.GetChild(0);
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }
            }

            ClearColourResults();

            // If we have a pathogen.
            if (foundPathogen != null)
            {
                if (ColourGuessPrefab != null)
                {
                    // Get the colour picker component from the ColourGuessPrefab
                    var itemPickerComp = ColourGuessPrefab.GetComponent<LetterPicker>();
                    var filteredAvailableItems = itemPickerComp.AvailableItems.Where(GetAvailableItemFilter()).ToList();
                    if (itemPickerComp != null)
                    {
                        // Get a count of available colours.
                        var availableColourCount = filteredAvailableItems.Count;

                        // Determine the difficulty of the pathogen
                        int minMatrixCount = Mathf.Clamp(3 + (foundPathogen.Severity - 1), 3, 6);

                        // build a colour matrix
                        for (int i = 0; i < minMatrixCount; i++)
                        {
                            CurrentTestItems.Add(filteredAvailableItems[UnityEngine.Random.Range(0, availableColourCount)]);

                            // Spawn the correct number of colour pickers.
                            var colourPickerElement = GameObject.Instantiate(ColourGuessPrefab);
                            colourPickerElement.transform.SetParent(ColourGuessItemList.transform);
                            CurrentItemPickers.Add(colourPickerElement);
                        }
                    }
                }
            }
        }
    }

    protected virtual Func<string, bool> GetAvailableItemFilter()
    {
        var rnaPathogen = CurrentPathogenInSynthesis.PathogenElements.Any(y => y.name.ToLowerInvariant().StartsWith("rna"));
        var dnaPathogen = CurrentPathogenInSynthesis.PathogenElements.Any(y => y.name.ToLowerInvariant().StartsWith("dna"));
        Func<string, bool> availableFilterClause = x => !string.IsNullOrWhiteSpace(x);
        if (rnaPathogen && !dnaPathogen)
        {
            availableFilterClause = x => x != "T";
        }
        else if (!rnaPathogen && dnaPathogen)
        {
            availableFilterClause = x => x != "U";
        }

        return availableFilterClause;
    }

    protected List<string> ExtractSelectedItemFromPickers(List<GameObject> CurrentLetterPickers)
    {
        var results = new List<string>();

        // For each colour element in the current colour pickers
        foreach (var cpElement in CurrentLetterPickers)
        {
            // Get the colour from the current guess colour picker.
            results.Add(cpElement.GetComponentInChildren<Text>().text);
        }

        return results;
    }
}
