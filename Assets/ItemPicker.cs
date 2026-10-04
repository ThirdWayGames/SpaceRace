using System.Collections.Generic;
using UnityEngine;

public abstract class ItemPicker<T> : MonoBehaviour
{
    public List<T> AvailableItems;

    public GameObject PickerPrefab;

    public GameObject PickerList;

    public GameObject PickerListContent;

    private List<GameObject> PickerElements;

    // Use this for initialization
    void Start ()
    {
        PickerElements = new List<GameObject>();
        if (PickerList != null)
        {
            // Get the rectTransform of the scroll list
            var cpLTrans = PickerList.GetComponent<RectTransform>();

            // Resize it based on the number of available colours.
            cpLTrans.sizeDelta = new Vector2(100, (100 * (AvailableItems.Count - 1)) + 20);

            // for each available colour
            foreach (var item in AvailableItems)
            {
                // Spawn all the available colours.
                var itemPrefab = Instantiate(PickerPrefab);

                // Set the colour of the new item.
                InitialiseNewItem(itemPrefab, item);

                // Set the result object for the colour element.
                var cpElementScript = itemPrefab.GetComponent<ItemPickerElement>();
                cpElementScript.ItemPickerResultElement = this.gameObject;

                // Add it to the list of available colours.
                itemPrefab.transform.SetParent(PickerListContent.transform);

                // Add the element to the list of elements.
                PickerElements.Add(itemPrefab);
            }
        }
    }

    public virtual void InitialiseNewItem(GameObject itemPrefab, T item)
    {
    }

    public void Display()
    {
        if (PickerList != null)
        {
            if (!PickerList.GetActive())
            {
                PickerList.SetActive(true);

                foreach (var element in PickerElements)
                {
                    element.GetComponent<ItemPickerElement>().Reset();
                }
            }
        }
    }

    public void Hide()
    {
        if (PickerList != null)
        {
            if (PickerList.GetActive())
            {
                PickerList.SetActive(false);
            }
        }
    }
}
