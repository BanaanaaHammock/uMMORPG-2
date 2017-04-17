// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UILoot : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] GameObject goldSlot;
    [SerializeField] Text goldText;
    [SerializeField] UILootSlot itemSlotPrefab;
    [SerializeField] Transform content;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // use collider point(s) to also work with big entities
        if (panel.activeSelf &&
            player.target != null &&
            player.target.health == 0 &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange &&
            player.target is Monster &&
            ((Monster)player.target).HasLoot()) {
            // cache monster
            var mob = (Monster)player.target;

            // gold slot
            if (mob.lootGold > 0) {
                goldSlot.SetActive(true);
                goldSlot.GetComponentInChildren<Button>().onClick.SetListener(() => {
                    player.CmdTakeLootGold();
                });
                goldText.text = mob.lootGold.ToString();
            } else goldSlot.SetActive(false);
           
            // instantiate/destroy enough slots
            // (we only want to show the non-empty slots)
            var items = mob.lootItems.Where(item => item.valid).ToList();
            UIUtils.BalancePrefabs(itemSlotPrefab.gameObject, items.Count, content);

            // refresh all valid items
            for (int i = 0; i < items.Count; ++i) {
                var slot = content.GetChild(i).GetComponent<UILootSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                int itemIndex = mob.lootItems.FindIndex(
                    item => item.valid && item.name == items[i].name
                );

                // refresh
                slot.button.interactable = player.InventoryCanAddAmount(items[i].template, items[i].amount);
                slot.button.onClick.SetListener(() => {
                    player.CmdTakeLootItem(itemIndex);
                });
                slot.tooltip.text = items[i].ToolTip();
                slot.image.color = Color.white;
                slot.image.sprite = items[i].image;                    
                slot.nameText.text = items[i].name;
                slot.amountOverlay.SetActive(items[i].amount > 1);
                slot.amountText.text = items[i].amount.ToString();
            }
        } else panel.SetActive(false); // hide
    }

    public void Show() { panel.SetActive(true); }
}
