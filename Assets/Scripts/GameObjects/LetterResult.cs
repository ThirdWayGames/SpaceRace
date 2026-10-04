using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameObjects
{
    public class LetterResult : BaseResult<string>
    {
        protected override bool Compare(string itemOne, string itemTwo)
        {
            return itemOne == itemTwo;
        }

        protected override void RedrawGuessedItem(int index, KeyValuePair<string, int> guessedItem)
        {
            // Get the colour from the current guess colour picker.
            // If the colour object from the colour result is not null.
            var itemElement = this.transform.GetChild(index).Find("ItemGuess");

            if (itemElement != null)
            {
                // Set the colour result using the current guess colour.
                itemElement.GetComponent<Text>().text = guessedItem.Key;
            }
        }
    }
}

