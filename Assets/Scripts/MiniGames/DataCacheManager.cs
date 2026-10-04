using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.UI;

public class DataCacheManager : MonoBehaviour, IConsoleSceneController
{
    public List<string> CurrentListOfFailingSystems;

    public List<string> CurrentListOfResetSystems;

    public List<Image> Stages;

    public SystemPanelManager SystemPanelManager;

    public Text FailedSystemDisplay;

    public Text AccessGrantedDisplay;

    public Text SystemLocked;

    public Text GameTimer;

    public AudioSource Alert;

    private IConsole CurrentConsole { get; set; }

    private float CurrentTimeTaken = 0;

    private DataCacheDifficultySettings DifficultySettings;

    private int CurrentSucessCount;

    private int CurrentFailingSystemCount;

    public void Awake()
    {
        if (SystemPanelManager == null)
        {
            Debug.LogWarning("No SystemPanelManager specified.");
            return;
        }

        if (FailedSystemDisplay == null)
        {
            Debug.LogWarning("No FailedSystemDisplay text object specified.");
            return;
        }
    }

    public void Update()
    {
        if (!SystemLocked.IsActive() && !AccessGrantedDisplay.IsActive())
        {
            CurrentTimeTaken -= Time.deltaTime;
            CurrentTimeTaken = Mathf.Clamp(CurrentTimeTaken, 0f, DifficultySettings.LockoutTime + (DifficultySettings.LockoutTimeMultiplier * CurrentConsole.GetFailureCount()));
            GameTimer.text = string.Format("TIME REMAINING: {0:F}", CurrentTimeTaken);
            UpdateFailedSystemsText();
        }

        // If we have not completed it and our time is up.
        if (!AccessGrantedDisplay.IsActive() && FailedSystemDisplay.gameObject.GetActive() && CurrentTimeTaken <= 0f)
        {
            // Display locked out.
            SystemLocked.gameObject.SetActive(true);
            SystemPanelManager.AlertComplete();
            FailedSystemDisplay.gameObject.SetActive(false);

            // Set all stages as failed.
            Stages.ForEach((image) => 
            {
                image.enabled = true; 
                image.color = Color.red;
            });

            // If the console is set and not locked.
            if (CurrentConsole != null && !CurrentConsole.IsLocked())
            {
                // Send the failed console result.
                CurrentConsole.SetConsoleResult(-1);
            }
        }
    }

    public void ButtonClicked(Button clicked)
    {
        var clickedText = clicked.GetComponentInChildren<Text>().text;
        if (!CurrentListOfResetSystems.Contains(clickedText))
        {
            CurrentListOfResetSystems.Add(clickedText);

            // If the user has failed
            if (FailedSystemCheck())
            {
                // Alert of failure.
                SystemPanelManager.AlertFailure(clicked);

                // Play the alert sound.
                Alert.Play();

                // Reset, the user gets a new set of systems.
                Reset(false);
            }
            else
            {
                if (SystemsRepairedCheck())
                {
                    CurrentSucessCount++;
                    Stages[Mathf.Clamp(CurrentSucessCount - 1, 0, Stages.Count)].enabled = true;
                    if (CurrentSucessCount >= DifficultySettings.SuccessAttempts)
                    {
                        SystemPanelManager.AlertComplete();
                        FailedSystemDisplay.gameObject.SetActive(false);
                        AccessGrantedDisplay.gameObject.SetActive(true);
                        CurrentConsole.SetConsoleResult(1);
                    }
                    else
                    {
                        // Reset, and advance.
                        Reset(true);
                    }
                }
                else
                {
                    SystemPanelManager.AlertSuccess(clicked);
                }
            }
        }
        else
        {
            Debug.Log(string.Format("Already clicked: '{0}'", clickedText));
        }
    }

    public void Reset(bool advanceToNextStage = false)
    {
        // Reset, the user gets a new set of systems.
        if (advanceToNextStage)
        {
            CurrentFailingSystemCount += UnityEngine.Random.Range(1, 3);
        }
        
        CurrentListOfFailingSystems = SystemPanelManager.GetRandomSystemFails(CurrentFailingSystemCount, GetButtonRestrictionCount());
        SystemPanelManager.Reset();
        UpdateFailedSystemsText();
        CurrentListOfResetSystems = new List<string>();
    }

    protected bool FailedSystemCheck()
    {
        foreach (var currentResetSystem in CurrentListOfResetSystems.Select((x, i) => new { Item = x, Index = i }))
        {
            var failingSystemAtIndex = CurrentListOfFailingSystems.Select((x, i) => new { Item = x, Index = i }).First(x => x.Index == currentResetSystem.Index);

            if (failingSystemAtIndex.Item != currentResetSystem.Item)
            {
                return true;
            }
        }

        return false;
    }

    protected bool SystemsRepairedCheck()
    {
        var results = false;
        if (!FailedSystemCheck())
        {
            results = CurrentListOfFailingSystems.Count == CurrentListOfResetSystems.Count;
        }

        return results;
    }

    protected void UpdateFailedSystemsText()
    {
        var failedSystemText = string.Join("\n", CurrentListOfFailingSystems.ToArray());
        FailedSystemDisplay.text = failedSystemText;
    }

    protected int? GetButtonRestrictionCount()
    {
        if (DifficultySettings.RestrictMaxButtons)
        {
            return DifficultySettings.MaxButtonCount;
        }

        return null;
    }

    public void InitaliseMiniGame(IConsole currentConsole)
    {
        CurrentConsole = currentConsole;
        if (CurrentConsole != null)
        {
            DifficultySettings = CurrentConsole.GetDifficultySettings() as DataCacheDifficultySettings;
            if (DifficultySettings == null)
            {
                Debug.LogError(string.Format("No Difficulty Settings detected for {0}", this.gameObject.name));
                DifficultySettings = new DataCacheDifficultySettings();
            }
        }
        else
        {
            Debug.LogError(string.Format("No Current Console set for {0} when activated", this.gameObject.name));
        }

        CurrentTimeTaken = DifficultySettings.LockoutTime + (DifficultySettings.LockoutTimeMultiplier * CurrentConsole.GetFailureCount());
        CurrentFailingSystemCount = DifficultySettings.StartingFailingSystemCount;
        CurrentListOfFailingSystems = SystemPanelManager.GetRandomSystemFails(CurrentFailingSystemCount, GetButtonRestrictionCount());
        CurrentListOfResetSystems = new List<string>();

        for (int i = 0; i < CurrentSucessCount; i++)
        {
            Stages[i].enabled = true;
            Debug.Log(string.Format("Stage {0} completed.", i + 1));
        }
    }
}
