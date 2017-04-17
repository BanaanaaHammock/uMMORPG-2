// Attach to the prefab for easier component access by the UI Scripts.
// Otherwise we would need slot.GetChild(0).GetComponentInChildren<Text> etc.
using UnityEngine;
using UnityEngine.UI;

public class UISkillSlot : MonoBehaviour {
    public UIShowToolTip tooltip;
    public UIDragAndDropable dragAndDropable;
    public Image image;
    public Button button;
    public GameObject cooldownOverlay;
    public Text cooldownText;
    public Text descriptionText;
    public Button learnButton;
}
