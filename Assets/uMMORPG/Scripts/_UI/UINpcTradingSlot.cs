// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

public class UINpcTradingSlot : MonoBehaviour {
    public UIShowToolTip tooltip;
    public Button button;
    public Image image;
}
