// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

public class UIBuffSlot : MonoBehaviour {
    public Image image;
    public UIShowToolTip tooltip;
    public Slider slider;
}
