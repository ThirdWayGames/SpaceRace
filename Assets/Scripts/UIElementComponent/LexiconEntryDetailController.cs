using Assets.Scripts.ScriptableObjects.Pathogens;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LexiconEntryDetailController : MonoBehaviour
{
    public GameObject LexiconEntryDetailImage;

    public GameObject LexiconEntryDetailName;

    public GameObject LexiconEntryDetailList;

    public GameObject LexiconEntryDetailPrefab;

    public ScriptableObject CurrentEntryItem;

    public Sprite DefaultBackground;

    /// <summary>
    /// Sets the current entry detail item to null.
    /// </summary>
    public void ClearCurrentEntryDetail()
    {
        CurrentEntryItem = null;

        this.GetComponent<Image>().sprite = DefaultBackground;
        var color = this.GetComponent<Image>().color;
        color.a = .3f;
        this.GetComponent<Image>().color = color;


        for (int i = 0; i < LexiconEntryDetailList.transform.childCount; i++)
        {
            Destroy(LexiconEntryDetailList.transform.GetChild(i).gameObject);
        }
    }

	// Use this for initialization
	public void LoadEntryItem(ScriptableObject entryItem)
    {
        ClearCurrentEntryDetail();

        // Cast the entry item as a Lexicon Entry.
        var castEntryItem = entryItem as LexiconEntry;

        // If its a valid type.
        if (castEntryItem != null)
        {
            // Set the values.
            LexiconEntryDetailName.GetComponent<Text>().text = castEntryItem.Name;
            if (castEntryItem.Image != null)
            {
                if (castEntryItem.SetImageAsBackground)
                {
                    this.GetComponent<Image>().sprite = castEntryItem.Image;
                    var color = this.GetComponent<Image>().color;
                    color.a = 1f;
                    this.GetComponent<Image>().color = color;

                    LexiconEntryDetailImage.GetComponent<Image>().enabled = false;
                }
                else
                {
                    LexiconEntryDetailImage.GetComponent<Image>().sprite = castEntryItem.Image;
                    LexiconEntryDetailImage.GetComponent<Image>().enabled = true;
                }
            }
            else
            {
                LexiconEntryDetailImage.GetComponent<Image>().enabled = false;
            }

            if (castEntryItem.ApplyImageTint)
            {
                LexiconEntryDetailImage.GetComponent<Image>().color = castEntryItem.ImageTint;
            }

            foreach (var entryDetailItem in castEntryItem.EntrySubDetails.Cast<LexiconEntryDetailItem>())
            {
                var entryItemInstance = Instantiate(LexiconEntryDetailPrefab);
                var detailImageLeft = entryItemInstance.GetComponentsInChildren<Image>().FirstOrDefault(x => x.name == "ImageLeft");
                if (detailImageLeft != null)
                {
                    detailImageLeft.gameObject.SetActive(entryDetailItem.ImageLeft != null);
                    if (entryDetailItem.ImageLeft != null)
                    {
                        var castTreatment = entryDetailItem.ImageLeft as PathogenElement;
                        if (castTreatment != null)
                        {

                            detailImageLeft.sprite = castTreatment.Shape;
                            detailImageLeft.color = castTreatment.Color;
                        }
                        else
                        {
                            Debug.LogWarningFormat("{0} is not of type 'PathogenElement'", entryDetailItem.ImageLeft.name);
                        }
                    }
                }

                var detailTitle = entryItemInstance.GetComponentsInChildren<Text>().FirstOrDefault(x => x.name == "Title");
                if (detailTitle != null)
                {
                    detailTitle.gameObject.SetActive(!string.IsNullOrWhiteSpace(entryDetailItem.Title));
                    detailTitle.text = entryDetailItem.Title;
                }

                var detailDescription = entryItemInstance.GetComponentsInChildren<Text>().FirstOrDefault(x => x.name == "Description");
                if (detailDescription != null)
                {
                    detailDescription.gameObject.SetActive(!string.IsNullOrWhiteSpace(entryDetailItem.Description));
                    detailDescription.text = entryDetailItem.Description;
                }

                var detailImageRight = entryItemInstance.GetComponentsInChildren<Image>().FirstOrDefault(x => x.name == "ImageRight");
                if (detailImageRight != null)
                {
                    detailImageRight.gameObject.SetActive(entryDetailItem.ImageRight != null);
                    if (entryDetailItem.ImageRight != null)
                    {
                        var castTreatment = entryDetailItem.ImageRight as PathogenElement;
                        if (castTreatment != null)
                        {

                            detailImageRight.sprite = castTreatment.Shape;
                            detailImageRight.color = castTreatment.Color;
                        }
                        else
                        {
                            Debug.LogWarningFormat("{0} is not of type 'PathogenElement'", entryDetailItem.ImageRight.name);
                        }
                    }
                }


                entryItemInstance.transform.SetParent(LexiconEntryDetailList.transform, false);
            }
        }

        CurrentEntryItem = entryItem;
    }

    private void Update()
    {
        var entryItem = CurrentEntryItem as LexiconEntry;
        var isValidEntry = entryItem != null;

        // Display the image and text if its valid.
        LexiconEntryDetailName.SetActive(isValidEntry && entryItem.Name != null);
        LexiconEntryDetailImage.SetActive(isValidEntry && entryItem.Image != null);
        LexiconEntryDetailList.SetActive(isValidEntry);
    }
}
