using UnityEngine;
using UnityEngine.EventSystems;

public class PathogenElementDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform DroppedIn = null;

    public bool InTargetZone;

    public void OnBeginDrag(PointerEventData eventData)
    {
        var instance = GameObject.Instantiate(gameObject);
        instance.GetComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
        var canvasParent = this.gameObject.GetComponentInParent<Canvas>();
        instance.transform.SetParent(canvasParent.transform);
        instance.GetComponent<CanvasGroup>().blocksRaycasts = false;
        instance.GetComponent<CanvasGroup>().alpha = .6f;
        eventData.pointerDrag = instance;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!InTargetZone)
        {
            eventData.pointerDrag.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DroppedIn != null)
        {
            eventData.pointerDrag.GetComponent<CanvasGroup>().blocksRaycasts = true;
            eventData.pointerDrag.GetComponent<CanvasGroup>().alpha = 1;

            // Remove the Drag Handler from the dropped element so it cant be dragged again.
            Destroy(eventData.pointerDrag.GetComponent<PathogenElementDragHandler>());
        }
        else
        {
            Destroy(eventData.pointerDrag);
        }
    }
}
