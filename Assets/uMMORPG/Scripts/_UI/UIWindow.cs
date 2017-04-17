// Adds window like behaviour to UI panels, so that they can be moved and closed
// by the user.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

enum CloseOption {
    DoNothing,
    DeactivateWindow,
    DestroyWindow
};

public class UIWindow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    // close option
    [SerializeField] CloseOption onClose = CloseOption.DeactivateWindow;

    // cache
    Transform window;

    void Awake() {
        // cache the parent window
        window = transform.parent;
        
        // add events
        var button = GetComponentInChildren<Button>(); // there is only one
        if (button != null) button.onClick.AddListener(OnClose);
    }
    
    public void HandleDrag(PointerEventData d) {
        // send message in case the parent needs to know about it
        window.SendMessage("OnWindowDrag", d, SendMessageOptions.DontRequireReceiver);
        
        // move the parent
        window.Translate(d.delta);
    }
    
    public void OnBeginDrag(PointerEventData d) {
        HandleDrag(d);
    }
    
    public void OnDrag(PointerEventData d) {
        HandleDrag(d);
    }
        
    public void OnEndDrag(PointerEventData d) {
        HandleDrag(d);
    }

    public void OnClose() {
        // send message in case it's needed
        // note: it's important to not name it the same as THIS function to avoid
        //       a deadlock
        window.SendMessage("OnWindowClose", SendMessageOptions.DontRequireReceiver);
        
        // hide window
        if (onClose == CloseOption.DeactivateWindow)
            window.gameObject.SetActive(false);
        
        // destroy if needed
        if (onClose == CloseOption.DestroyWindow)
            Destroy(window.gameObject);
    }
}
