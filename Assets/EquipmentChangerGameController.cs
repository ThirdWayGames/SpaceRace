using Assets.Scripts.GameObjects;
using Assets.Scripts.Interfaces;
using Assets.Scripts.ScriptableObjects;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EquipmentChangerGameController : MonoBehaviour, IConsoleSceneController
{
    public ScriptableObject ClientEquipmentLoadout;

    public GameObject PathogenListLeft;

    public GameObject PathogenListRight;

    public GameObject PathogenSelectButtonPrefab;

    public Canvas UiCanvas;

    public int TeamId;

    private IConsole CurrentConsole { get; set; }

    public void Update()
    {
        if (UiCanvas != null && ClientEquipmentLoadout != null)
        {
            var equimentLoadout = ClientEquipmentLoadout as EquipmentLoadout;
            if (equimentLoadout != null)
            {
                if (equimentLoadout.LeftHandEquipment != null)
                {
                    ProcessEquipmentButtons(equimentLoadout.LeftHandEquipment, "Left");
                }

                if (equimentLoadout.RightHandEquipment != null)
                {
                    ProcessEquipmentButtons(equimentLoadout.RightHandEquipment, "Right");
                }
            }
        }
    }

    public void TogglePathogenListLeft(bool display)
    {
        if (PathogenListLeft != null)
        {
            PathogenListLeft.SetActive(display);
        }
    }

    public void TogglePathogenListRight(bool display)
    {
        if (PathogenListRight != null)
        {
            PathogenListRight.SetActive(display);
        }
    }

    public void SetLeftEquipment(string resource)
    {
        var pathogen = string.Empty;
        var equipment = string.Empty;
        if (!resource.Contains("|"))
        {
            equipment = resource;
            TogglePathogenListLeft(false);
        }
        else
        {
            equipment = resource.Split('|')[0];
            pathogen = resource.Split('|')[1];
        }

        var resourceGo = Resources.Load(equipment) as GameObject;
        if (resourceGo == null)
        {
            Debug.LogWarning(string.Format("Can not find '{0}' as a resource or is not a prefab.", resource));
        }

        var equipmentLoadout = ClientEquipmentLoadout as EquipmentLoadout;
        if (equipmentLoadout != null && resourceGo != null)
        {
            // If the pathogen has been set.
            if (!string.IsNullOrWhiteSpace(pathogen))
            {
                // Get the pathogen and the pathogen gun compoenent.
                var pathogenOject = Resources.Load(pathogen) as ScriptableObject;
                var pathogenGun = resourceGo.GetComponent<PathogenGun>();

                // If we have them both.
                if (pathogenGun != null && pathogenOject != null)
                {
                    // Set the pathogen.
                    pathogenGun.Pathogen = pathogenOject;
                }
            }
            else
            {
                PathogenListLeft.SetActive(false);
            }

            equipmentLoadout.LeftHandEquipment = resourceGo;
        }
    }

    public void SetRightEquipment(string resource)
    {
        var pathogen = string.Empty;
        var equipment = string.Empty;
        if (!resource.Contains("|"))
        {
            equipment = resource;
            TogglePathogenListRight(false);
        }
        else
        {
            equipment = resource.Split('|')[0];
            pathogen = resource.Split('|')[1];
        }

        var resourceGo = Resources.Load(equipment) as GameObject;
        if (resourceGo == null)
        {
            Debug.LogWarning(string.Format("Can not find '{0}' as a resource or is not a prefab.", resource));
        }

        var equipmentLoadout = ClientEquipmentLoadout as EquipmentLoadout;
        if (equipmentLoadout != null && resourceGo != null)
        {
            // If the pathogen has been set.
            if (!string.IsNullOrWhiteSpace(pathogen))
            {
                // Get the pathogen and the pathogen gun compoenent.
                var pathogenOject = Resources.Load(pathogen) as ScriptableObject;
                var pathogenGun = resourceGo.GetComponent<PathogenGun>();

                // If we have them both.
                if (pathogenGun != null && pathogenOject != null)
                {
                    // Set the pathogen.
                    pathogenGun.Pathogen = pathogenOject;
                }
            }
            else
            {
                PathogenListRight.SetActive(false);
            }

            equipmentLoadout.RightHandEquipment = resourceGo;
        }
    }

    public void QuitConsole()
    {
        if (CurrentConsole != null)
        {
            CurrentConsole.SetConsoleResult(1);
        }
    }

    protected void ProcessEquipmentButtons(GameObject equipment, string side)
    {
        var buttons = UiCanvas.GetComponentsInChildren<Button>();
        foreach (var currentButton in buttons.Where(x => x.name.StartsWith(side)))
        {
            // Get the current button colours.
            var buttonStyle = currentButton.colors;

            // If the button we are processing matches the equipment.
            if (equipment.name.StartsWith(currentButton.name.Replace(side, string.Empty)))
            {
                // Make it green
                buttonStyle.normalColor = new Color(0, 200, 0, 255);
                buttonStyle.highlightedColor = new Color(0, 255, 0, 255);
                buttonStyle.pressedColor = new Color(0, 230, 0, 255);

                // If a pathogen gun is selected.
                if (equipment.name.ToLowerInvariant().Contains("pathogen"))
                {
                    if (side.ToLowerInvariant() == "left")
                    {
                        TogglePathogenListLeft(true);
                        ProcessPathogenButtons(equipment, PathogenListLeft);
                    }
                    else
                    {
                        TogglePathogenListRight(true);
                        ProcessPathogenButtons(equipment, PathogenListRight);
                    }
                }
            }
            else
            {
                buttonStyle.normalColor = new Color(255, 255, 255, 255);
                buttonStyle.highlightedColor = new Color(245, 245, 245, 255);
                buttonStyle.pressedColor = new Color(200, 200, 200, 255);
            }

            currentButton.colors = buttonStyle;
        }

    }

    public void InitaliseMiniGame(IConsole currentConsole)
    {
        CurrentConsole = currentConsole;

        // Load all the pathogens into the pathogen lists.
        var pathogenLib = FindObjectOfType<PathogenLibrary>();
        if (CurrentConsole != null)
        {
            var activeConsolePlayer = CurrentConsole.GetActivePlayer();
            if (activeConsolePlayer != null)
            {
                TeamId = activeConsolePlayer.GetTeamId();
            }
        }
        
        if (pathogenLib != null)
        {
            // Providing I have a pathogen
            var knowPathogens = pathogenLib.TeamKnownPathogens.FirstOrDefault(x => x.Key == TeamId).Value;

            if (knowPathogens != null && knowPathogens.Any())
            {
                foreach (var pathogen in knowPathogens)
                {
                    var pathogenButtonLeft = Instantiate(PathogenSelectButtonPrefab);
                    var pathSelectorButtonCompLeft = pathogenButtonLeft.GetComponent<PathogenSelectorButton>();
                    pathSelectorButtonCompLeft.PathogenGunName += "Left";
                    pathSelectorButtonCompLeft.EquipmentChangerGameController = this;
                    pathSelectorButtonCompLeft.Initalise(pathogen, true);
                    pathogenButtonLeft.transform.SetParent(PathogenListLeft.GetComponentInChildren<ContentSizeFitter>().transform);

                    var pathogenButtonRight = Instantiate(PathogenSelectButtonPrefab);
                    var pathSelectorButtonCompRight = pathogenButtonRight.GetComponent<PathogenSelectorButton>();
                    pathSelectorButtonCompRight.PathogenGunName += "Right";
                    pathSelectorButtonCompRight.EquipmentChangerGameController = this;
                    pathSelectorButtonCompRight.Initalise(pathogen, false);
                    pathogenButtonRight.transform.SetParent(PathogenListRight.GetComponentInChildren<ContentSizeFitter>().transform);
                }
            }
        }
        else
        {
            // No pathogen library found in scene.
            Debug.LogError("No PathogenLibrary is present in scene.");
        }
    }

    public void LoadScene(string sceneName)
    {
        // Load the assoc. minigame scene.
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
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

    protected void ProcessPathogenButtons(GameObject equipment, GameObject pathogenButtonList)
    {
        for (int i = 0; i < pathogenButtonList.GetComponentInChildren<ContentSizeFitter>().transform.childCount; i++)
        {
            var pathogenButton = pathogenButtonList.GetComponentInChildren<ContentSizeFitter>().transform.GetChild(i).GetComponent<PathogenSelectorButton>();
            var pathogenGun = equipment.GetComponent<PathogenGun>();
            if (pathogenGun != null && pathogenButton != null)
            {
                pathogenButton.ToggleButtonSelect(pathogenGun.Pathogen.name == pathogenButton.ButtonText.text);
            }
        }
    }
}