using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PathogenSelectorButton : MonoBehaviour
{
    public Text ButtonText;

    public Button ButtonComponent;

    public string PathogenGunName = "PathogenBlaster";

    public EquipmentChangerGameController EquipmentChangerGameController;

    public Color SelectedColour;

    private Color UnselectedColour;

    public void Awake()
    {
        UnselectedColour = this.GetComponent<Image>().color;
    }

    public void ToggleButtonSelect(bool selected)
    {
        this.GetComponent<Image>().color = selected ? SelectedColour : UnselectedColour;
    }

    public void Initalise(string buttonText, bool isLeft)
    {
        ButtonText.text = buttonText;
        ButtonComponent.onClick.AddListener(delegate {  ExecuteButton(buttonText, isLeft); });
    }

    public void ExecuteButton(string buttonText, bool isLeft)
    {
        var param = string.Format("{0}|{1}", PathogenGunName, buttonText);
        if (isLeft)
        {
            EquipmentChangerGameController.SetLeftEquipment(param);
        }
        else
        {
            EquipmentChangerGameController.SetRightEquipment(param);
        }
    }
}
