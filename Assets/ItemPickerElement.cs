using UnityEngine;
using UnityEngine.EventSystems;

public abstract class ItemPickerElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject ItemPickerResultElement;

    public GameObject ItemElementSelected;

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemElementSelected.SetActive(true);
        SetParentItem();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemElementSelected.SetActive(false);
    }

    public abstract void SetParentItem();

    public void Reset()
    {
        ItemElementSelected.SetActive(false);
    }

    // Use this for initialization
    void Start ()
    {
        Reset();
    }
}
