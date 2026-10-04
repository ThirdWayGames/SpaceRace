using UnityEngine;
using UnityEngine.EventSystems;

public class PathogenElementDropHandler : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool SocketLocked = false;

    public Texture2D LockedCursorIcon;

    public void OnDrop(PointerEventData eventData)
    {
        // If the socket is not locked allow dropping.
        if (eventData.pointerDrag != null && !SocketLocked)
        {
            // If this socket already contains a child
            if (this.transform.childCount > 0)
            {
                // Get the child.
                var child = this.transform.GetChild(0);

                // Orphan the child
                child.SetParent(null);

                // Destroy the child.
                Destroy(child.gameObject);
            }

            // Add the dropped child to the socket.
            eventData.pointerDrag.transform.SetParent(this.transform);
            eventData.pointerDrag.GetComponent<PathogenElementDragHandler>().DroppedIn = this.transform;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // If the socket is locked
        if (SocketLocked)
        {
            // Change the cursor to a locked state.
            Cursor.SetCursor(LockedCursorIcon, Vector2.zero, CursorMode.Auto);
        }

        // If we have pointer drag data and the socket is not locked allow snapping.
        if (eventData.pointerDrag != null && !SocketLocked)
        {
            // Set the in target zone to true on the dragged element.
            eventData.pointerDrag.GetComponent<PathogenElementDragHandler>().InTargetZone = true;

            // Snap the elements pos to the center of the socket.
            eventData.pointerDrag.transform.position = this.transform.position;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset the cursor
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

        if (eventData.pointerDrag != null)
        {
            // Set the in target zone to false. (this will force the dragged element to snap to the mouse pointer)
            eventData.pointerDrag.GetComponent<PathogenElementDragHandler>().InTargetZone = false;
        }
    }
}
