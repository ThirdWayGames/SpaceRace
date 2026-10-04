using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColourPicker : ItemPicker<Color>
{
    public override void InitialiseNewItem(GameObject itemPrefab, Color item)
    {
        // Set the colour of the new item.
        var cpImage = itemPrefab.GetComponent<Image>();
        cpImage.color = item;
    }
}
