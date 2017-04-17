// Drag and Drop support for UI elements. Drag and Drop actions will be sent to
// the PlayerDndHandler component.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIDragAndDropable : MonoBehaviour , IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler {
    // drag options
    [SerializeField] PointerEventData.InputButton button = PointerEventData.InputButton.Left;
    [SerializeField] GameObject drageePrefab;
    GameObject currentlyDragged;

    // status
    public bool dragable = true;
    public bool dropable = true;
    
    [HideInInspector] public bool draggedToSlot = false;    
    
    public void OnBeginDrag(PointerEventData d) {
        // one mouse button is enough for dnd
        if (dragable && d.button == button) {
            // load current
            currentlyDragged = (GameObject)Instantiate(drageePrefab, transform.position, Quaternion.identity);
            currentlyDragged.GetComponent<Image>().sprite = GetComponent<Image>().sprite;
            currentlyDragged.transform.SetParent(transform.root, true); // canvas
            currentlyDragged.transform.SetAsLastSibling(); // move to foreground
        }
    }
    
    public void OnDrag(PointerEventData d) {
        // one mouse button is enough for drag and drop
        if (dragable && d.button == button)
            // move current
            currentlyDragged.transform.position = d.position;
    }
    
    // called after the slot's OnDrop
    public void OnEndDrag(PointerEventData d) {            
        // delete current in any case
        Destroy(currentlyDragged);

        // one mouse button is enough for drag and drop
        if (dragable && d.button == button) {
            // try destroy if not dragged to a slot (flag will be set by slot)
            // message is sent to drag and drop handler for game specifics
            if (!draggedToSlot)
                FindObjectOfType<PlayerDndHandling>().OnDragAndClear(tag, name.ToInt());
            
            // reset flag
            draggedToSlot = false;
        }
    }

    // d.pointerDrag is the object that was dragged
    public void OnDrop(PointerEventData d) {
        // one mouse button is enough for drag and drop
        if (dropable && d.button == button) {
            // was the dropped GameObject a UIDragAndDropable?
            var dropDragable = d.pointerDrag.GetComponent<UIDragAndDropable>();
            if (dropDragable) {
                // let the dragable know that it was dropped onto a slot
                dropDragable.draggedToSlot = true;

                // only do something if we didn't drop it on itself. this way we
                // don't have to ignore raycasts etc.
                // message is sent to drag and drop handler for game specifics
                if (dropDragable != this)
                    FindObjectOfType<PlayerDndHandling>().OnDragAndDrop(dropDragable.tag,
                                                                        dropDragable.name.ToInt(),
                                                                        tag,
                                                                        name.ToInt());
            }
        }
    }

    void OnDisable() {
        Destroy(currentlyDragged);
    }

    void OnDestroy() {
        Destroy(currentlyDragged);
    }
}
