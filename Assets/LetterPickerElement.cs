using UnityEngine.UI;

public class LetterPickerElement : ItemPickerElement
{
    public override void SetParentItem()
    {
        // Set the parents colour;
        var letterPickerResultTxt = ItemPickerResultElement.GetComponentInChildren<Text>();
        if (letterPickerResultTxt != null)
        {
            letterPickerResultTxt.text = this.GetComponent<Text>().text;
        }
    }
}
