// Instantiates a tooltip while the cursor is over this UI element.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIShowToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [SerializeField] GameObject tooltipPrefab;
    [TextArea(1, 30)] public string text = "";
    
    // instantiated tooltip
    GameObject current;

    void CreateToolTip() {
        // instantiate
        current = (GameObject)Instantiate(tooltipPrefab, transform.position, Quaternion.identity);
        
        // put to foreground
        current.transform.SetParent(transform.root, true); // canvas
        current.transform.SetAsLastSibling(); // last one means foreground
        
        // print the text
        current.GetComponentInChildren<Text>().text = text;
    }
    
    public void OnPointerEnter(PointerEventData d) {
        Invoke("CreateToolTip", 0.5f);
    }

    void DestroyToolTip() {
        // stop any running attempts to show it
        CancelInvoke("CreateToolTip");

        // destroy it
        Destroy(current);
    }
    
    public void OnPointerExit(PointerEventData d) {
        DestroyToolTip();
    }

    void OnDisable() {
        DestroyToolTip();
    }

    void OnDestroy() {
        DestroyToolTip();
    }
}
