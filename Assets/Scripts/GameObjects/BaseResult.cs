using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.GameObjects
{
    public abstract class BaseResult<T> : MonoBehaviour
    {
        public Text Correct;

        public Text Misplaced;

        protected List<ItemLocator<T>> GuessedItemList;

        protected List<ItemLocator<T>> HiddenItemList;

        protected List<ItemLocator<T>> CorrectItems = new List<ItemLocator<T>>();

        protected List<ItemLocator<T>> MisplacedItems = new List<ItemLocator<T>>();

        public void Awake()
        {
            ResetItemResult();
        }

        /// <summary>
        /// Resets the colour result item.
        /// </summary>
        public virtual void ResetItemResult()
        {
            GuessedItemList = new List<ItemLocator<T>>();
            HiddenItemList = new List<ItemLocator<T>>();
        }

        /// <summary>
        /// Sets the colour items colours based on the current colour pickers.
        /// </summary>
        /// <param name="currentColourPickers"></param>
        /// <param name="hiddenColours">The hidden colours to test against.</param>
        public virtual void SetResults(List<T> currentColourPickers, List<T> hiddenColours)
        {
            ResetItemResult();

            // Hide the elements that are beyond the colour picker count.
            for (var i = this.transform.childCount - 2; i >= currentColourPickers.Count; i--)
            {
                // For each element that isn't the result output.
                if (this.transform.GetChild(i).name != "ResultOutput")
                {
                    this.transform.GetChild(i).gameObject.SetActive(false);
                }
            }

            // For each colour element in the current colour pickers
            for (var i = 0; i < hiddenColours.Count; i++)
            {
                HiddenItemList.Add(new ItemLocator<T> { value = hiddenColours[i], itemIndex = i });
            }

            for (var i = 0; i < currentColourPickers.Count; i++)
            {
                var guessedColour = new KeyValuePair<T, int>(currentColourPickers[i], 2);
                GuessedItemList.Add(new ItemLocator<T> { value = currentColourPickers[i], itemIndex = i });
                RedrawGuessedItem(i, guessedColour);
            }
        }

        /// <summary>
        /// Processes the result of the guessed colours against the hidden colours and flags accordingly.
        /// </summary>
        /// <returns>True of the result matches the hidden colours.</returns>
        public virtual bool ProcessResult()
        {
            // Perform first pass
            ProcessItems();

            // Set the correct and Missplaced texts
            Correct.text = string.Format("{0}", CorrectItems.Count);
            Misplaced.text = string.Format("{0}", MisplacedItems.Count());

            // Return true if all the hidden colours have been evaluated correctly in the correct place (all values = 0).
            return CorrectItems.Count == HiddenItemList.Count;
        }

        protected virtual void ProcessItems()
        {
            // For each guessed item.
            for (int guessIndex = 0; guessIndex < GuessedItemList.Count; guessIndex++)
            {
                // Get the guessed item
                var guessedItem = GuessedItemList[guessIndex];

                var foundItemInHidden = HiddenItemList.Where(x => Compare(x.value, guessedItem.value)).ToList();

                // for each item in the hidden values
                foreach (var foundItem in foundItemInHidden)
                {
                    // if the match is located at the same index (correctly placed)
                    if (guessIndex == HiddenItemList.IndexOf(foundItem))
                    {
                        // if we have placed it in the misplaced items already.
                        var misplacedToRemove = MisplacedItems.Where(x => x.itemIndex == foundItem.itemIndex).ToList();
                        if (misplacedToRemove.Any())
                        {
                            for (int misplacedToRemoveIdx = 0; misplacedToRemoveIdx < misplacedToRemove.Count; misplacedToRemoveIdx++)
                            {
                                // Remove it from the misplaced items
                                MisplacedItems.Remove(MisplacedItems[misplacedToRemoveIdx]);
                            }
                        }

                        // Add it to the correctly matched list.
                        guessedItem.hiddenMatchIndex = foundItem.itemIndex;
                        CorrectItems.Add(guessedItem);
                        break;
                    }
                    else
                    {
                        // If the item has been been flagged as a misplaced item for the current hidden item.
                        if (!MisplacedItems.Any(x => Compare(x.value, guessedItem.value) && x.itemIndex == guessedItem.itemIndex && x.hiddenMatchIndex == foundItem.itemIndex))
                        {
                            // Add it to the misplaced list for this hidden item.
                            MisplacedItems.Add(new ItemLocator<T> { value = guessedItem.value, itemIndex = guessedItem.itemIndex, hiddenMatchIndex = foundItem.itemIndex });
                        }
                    }
                }
            }

            // Remove any misplaced items that have actually been matched correctly but left in the misplaced items.
            for (int correctIndex = 0; correctIndex < CorrectItems.Count; correctIndex++)
            {
                var misplacedItemsThatsActuallyCorrect = MisplacedItems.Where(x => x.hiddenMatchIndex == CorrectItems[correctIndex].itemIndex).ToList();

                // If we have already added an item as misplaced for this found item index.
                if (misplacedItemsThatsActuallyCorrect.Any())
                {
                    foreach (var iteam in misplacedItemsThatsActuallyCorrect)
                    {
                        // Remove it as we are about to mark it as correctly placed.
                        MisplacedItems.Remove(iteam);
                    }
                }
            }

            // Remove any duplcate that have resulted from multiple items in the hidden list.
            var groupCount = MisplacedItems.GroupBy(x => new { x.value, x.itemIndex }).Where(g => g.Count() > 1).Count();
            while (groupCount > 0)
            {
                var dupeItem = MisplacedItems.GroupBy(x => new { x.value, x.itemIndex }).Where(g => g.Count() > 1).First();

                // Use the group to get the misplaced items
                var itemToKeep = MisplacedItems.Where(x => Compare(x.value, dupeItem.Key.value) && x.itemIndex == dupeItem.Key.itemIndex).OrderBy(x => x.hiddenMatchIndex).First();

                // Remove any other items that match either the letter/itemIndex combo or the letter/hiddenIndex combo
                var tmpMisplacedList = MisplacedItems.Where(x => (Compare(x.value, itemToKeep.value) && x.itemIndex == itemToKeep.itemIndex && x.hiddenMatchIndex != itemToKeep.hiddenMatchIndex) || (Compare(x.value, itemToKeep.value) && x.itemIndex != itemToKeep.itemIndex && x.hiddenMatchIndex == itemToKeep.hiddenMatchIndex)).ToList();
                foreach (var itemToRemove in tmpMisplacedList)
                {
                    MisplacedItems.Remove(itemToRemove);
                }

                // Recalc the group count.
                groupCount = MisplacedItems.GroupBy(x => new { x.value, x.itemIndex }).Where(g => g.Count() > 1).Count();
            }

            // Remove any duplcate that have resulted from multiple items in the guessed list.
            groupCount = MisplacedItems.GroupBy(x => new { x.value, x.hiddenMatchIndex }).Where(g => g.Count() > 1).Count();
            while (groupCount > 0)
            {
                var dupeItem = MisplacedItems.GroupBy(x => new { x.value, x.hiddenMatchIndex }).Where(g => g.Count() > 1).First();

                // Use the group to get the misplaced items
                var itemToKeep = MisplacedItems.Where(x => Compare(x.value, dupeItem.Key.value) && x.hiddenMatchIndex == dupeItem.Key.hiddenMatchIndex).OrderBy(x => x.itemIndex).First();

                // Remove any other items that match either the letter/itemIndex combo or the letter/hiddenIndex combo
                var tmpMisplacedList = MisplacedItems.Where(x => (Compare(x.value, itemToKeep.value) && x.itemIndex == itemToKeep.itemIndex && x.hiddenMatchIndex != itemToKeep.hiddenMatchIndex) || (Compare(x.value, itemToKeep.value) && x.itemIndex != itemToKeep.itemIndex && x.hiddenMatchIndex == itemToKeep.hiddenMatchIndex)).ToList();
                foreach (var itemToRemove in tmpMisplacedList)
                {
                    MisplacedItems.Remove(itemToRemove);
                }

                // Recalc the group count.
                groupCount = MisplacedItems.GroupBy(x => new { x.value, x.hiddenMatchIndex }).Where(g => g.Count() > 1).Count();
            }
        }

        protected abstract bool Compare(T itemOne, T itemTwo);

        protected abstract void RedrawGuessedItem(int index, KeyValuePair<T, int> guessedColour);
    }

    public class ItemLocator<T>
    {
        public T value;

        public int itemIndex;

        public int hiddenMatchIndex;

        public virtual bool IsEqual(T item)
        {
            return value.ToString() == item.ToString();
        }
    }
}