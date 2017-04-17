// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public class UIInventory : MonoBehaviour {
    [SerializeField] KeyCode hotKey = KeyCode.I;
    [SerializeField] GameObject panel;
    [SerializeField] UIInventorySlot slotPrefab;
    [SerializeField] Transform content;
    [SerializeField] Text goldText;
    [SerializeField] UIDragAndDropable trash;
    [SerializeField] Image trashImage;
    [SerializeField] GameObject trashOverlay;
    [SerializeField] Text trashAmountText;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only update the panel if it's active
        if (panel.activeSelf) {
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.inventory.Count, content);

            // refresh all items
            for (int i = 0; i < player.inventory.Count; ++i) {
                var slot = content.GetChild(i).GetComponent<UIInventorySlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                var item = player.inventory[i];

                if (item.valid) {
                    // refresh valid item
                    int icopy = i; // needed for lambdas, otherwise i is Count
                    slot.button.onClick.SetListener(() => {
                        if (player.level >= item.minLevel)
                            player.CmdUseInventoryItem(icopy);
                    });
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = item.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = item.image;
                    slot.amountOverlay.SetActive(item.amount > 1);
                    slot.amountText.text = item.amount.ToString();
                } else {
                    // refresh invalid item
                    slot.button.onClick.RemoveAllListeners();
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }

            // gold
            goldText.text = player.gold.ToString();

            // trash (tooltip always enabled, dropable always true)
            trash.dragable = player.trash.valid;
            if (player.trash.valid) {
                // refresh valid item
                trashImage.color = Color.white;
                trashImage.sprite = player.trash.image;
                trashOverlay.SetActive(player.trash.amount > 1);
                trashAmountText.text = player.trash.amount.ToString();
            } else {
                // refresh invalid item
                trashImage.color = Color.clear;
                trashImage.sprite = null;
                trashOverlay.SetActive(false);
            }
        }
    }
}
