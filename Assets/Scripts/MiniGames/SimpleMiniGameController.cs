using Assets.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SimpleMiniGameController : MonoBehaviour, IConsoleSceneController
{
    private IConsole CurrentConsole { get; set; }

    // Update is called once per frame
    public void CompleteClicked()
    {
        if (CurrentConsole != null)
        {
            CurrentConsole.SetConsoleResult(1);
        }
    }

    public void FailClicked()
    {
        if (CurrentConsole != null)
        {
            CurrentConsole.SetConsoleResult(-1);
        }
    }

    /// <summary>
    /// Called when the user quits from the console.
    /// </summary>
    public void QuitConsole()
    {
        SceneManager.UnloadSceneAsync(this.gameObject.scene.name);
    }

    public void InitaliseMiniGame(IConsole currentConsole)
    {
        CurrentConsole = currentConsole;
    }
}