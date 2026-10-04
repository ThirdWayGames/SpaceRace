using Assets.Scripts.ScriptableObjects.Pathogens;
using UnityEngine;
using UnityEngine.UI;

public class PathogenElementDisplay : MonoBehaviour
{
    public Text TypeText;
    public Image Shape;

    public ScriptableObject PathogenElement;

	// Use this for initialization
	void Awake ()
    {
        var pathogenElement = PathogenElement as PathogenElement;

        if (pathogenElement != null)
        {
            TypeText.text = pathogenElement.GetName();
            TypeText.color = pathogenElement.TextColor;
            Shape.sprite = pathogenElement.Shape;
            Shape.color = pathogenElement.Color;
        }
    }
}
