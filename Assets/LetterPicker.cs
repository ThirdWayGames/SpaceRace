using UnityEngine;
using UnityEngine.UI;

public class LetterPicker : ItemPicker<string>
{
    public override void InitialiseNewItem(GameObject itemPrefab, string item)
    {
        // Set the colour of the new item.
        var lpText = itemPrefab.GetComponent<Text>();
        lpText.text = item;
    }
}
