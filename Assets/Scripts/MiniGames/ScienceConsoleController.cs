using Assets.Scripts.Enums;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects.Pathogens;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScienceConsoleController : MonoBehaviour, IConsoleSceneController
{
    /// <summary>
    /// A list of LexiconEntries
    /// </summary>
    public List<string> ButtonScenes;

    /// <summary>
    /// The current console that triggered the scene loading.
    /// </summary>
    private IConsole CurrentConsole { get; set; }

    /// <summary>
    /// Called when the user quits from the console.
    /// </summary>
    public void QuitConsole()
    {
        if (CurrentConsole != null)
        {
            CurrentConsole.SetConsoleResult(1, true);
        }
    }

    /// <summary>
    /// Loads all the current entries of a specified type into the list.
    /// </summary>
    /// <param name="lexiconEntryType"></param>
    public void LoadScene(int sceneIndex)
    {
        // Load the assoc. minigame scene.
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(ButtonScenes[sceneIndex], LoadSceneMode.Additive);
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.SetActiveScene(scene);

        var sceneGameManager = scene.GetRootGameObjects().ToList().FirstOrDefault(x => x.name == "GameManager");
        if (sceneGameManager != null)
        {
            var consoleSceneController = sceneGameManager.GetComponent<IConsoleSceneController>();
            if (consoleSceneController != null)
            {
                consoleSceneController.InitaliseMiniGame(CurrentConsole);
            }
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
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
