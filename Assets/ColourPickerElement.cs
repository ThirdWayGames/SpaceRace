using UnityEngine.UI;

public class ColourPickerElement : ItemPickerElement
{
    public override void SetParentItem()
    {
        // Set the parents colour;
        var colourPickerResultImg = ItemPickerResultElement.GetComponent<Image>();
        colourPickerResultImg.color = this.GetComponent<Image>().color;
    }
}
