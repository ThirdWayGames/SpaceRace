using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameObjects
{
    public class ColourResult : BaseResult<Color>
    {
        protected override bool Compare(Color itemOne, Color itemTwo)
        {
            return itemOne == itemTwo;
        }

        protected override void RedrawGuessedItem(int index, KeyValuePair<Color, int> guessedColour)
        {
            // Get the colour from the current guess colour picker.
            // If the colour object from the colour result is not null.
            var colourElement = this.transform.GetChild(index).Find("Colour");

            if (colourElement != null)
            {
                // Set the colour result using the current guess colour.
                colourElement.GetComponent<Image>().color = guessedColour.Key;
            }
        }
    }
}