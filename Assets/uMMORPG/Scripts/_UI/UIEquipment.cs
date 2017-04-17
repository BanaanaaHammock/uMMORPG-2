// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public class UIEquipment : MonoBehaviour {
    [SerializeField] KeyCode hotKey = KeyCode.E;
    [SerializeField] GameObject panel;
    [SerializeField] UIEquipmentSlot slotPrefab;
    [SerializeField] Transform content;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only update the panel if it's active
        if (panel.activeSelf) {
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.equipment.Count, content);

            // refresh all
            for (int i = 0; i < player.equipment.Count; ++i) {
                var slot = content.GetChild(i).GetComponent<UIEquipmentSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop slot
                var item = player.equipment[i];

                // set category overlay in any case. we use the last noun in the
                // category string, for example EquipmentWeaponBow => Bow
                // (disabled if no category, e.g. for archer shield slot)
                slot.categoryOverlay.SetActive(player.equipmentTypes[i] != "");
                string overlay = Utils.ParseLastNoun(player.equipmentTypes[i]);
                slot.categoryText.text = overlay != "" ? overlay : "?";

                if (item.valid) {
                    // refresh valid item
                    slot.tooltip.enabled = item.valid;
                    slot.tooltip.text = item.ToolTip();
                    slot.dragAndDropable.dragable = item.valid;
                    slot.image.color = Color.white;
                    slot.image.sprite = item.image;
                } else {
                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                }
            }
        }
    }
}
