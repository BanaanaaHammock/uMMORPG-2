using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UICrafting : MonoBehaviour {
    [SerializeField] KeyCode hotKey = KeyCode.T;
    [SerializeField] GameObject panel;
    [SerializeField] UICraftingIngredientSlot ingredientSlotPrefab;
    [SerializeField] Transform ingredientContent;
    [SerializeField] Image resultSlotImage;
    [SerializeField] UIShowToolTip resultSlotToolTip;
    [SerializeField] Button craftButton;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only update the panel if it's active
        if (panel.activeSelf) {
            // instantiate/destroy enough slots
            UIUtils.BalancePrefabs(ingredientSlotPrefab.gameObject, player.craftingIndices.Count, ingredientContent);

            // refresh all
            for (int i = 0; i < player.craftingIndices.Count; ++i) {
                var slot = ingredientContent.GetChild(i).GetComponent<UICraftingIngredientSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                int itemIndex = player.craftingIndices[i];

                if (0 <= itemIndex && itemIndex < player.inventory.Count &&
                    player.inventory[itemIndex].valid) {
                    var item = player.inventory[itemIndex];

                    // refresh valid item
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = item.ToolTip();
                    slot.dragAndDropable.dragable = true;
                    slot.image.color = Color.white;
                    slot.image.sprite = item.image;
                } else {
                    // reset the index because it's invalid
                    player.craftingIndices[i] = -1;

                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                }
            }

            // find valid indices => item templates => matching recipe
            var validIndices = player.craftingIndices.Where(
                idx => 0 <= idx && idx < player.inventory.Count && 
                       player.inventory[idx].valid
            );
            var items = validIndices.Select(idx => player.inventory[idx].template).ToList();
            var recipe = RecipeTemplate.dict.Values.ToList().Find(r => r.CanCraftWith(items)); // good enough for now
            if (recipe != null) {
                // refresh valid recipe
                resultSlotToolTip.enabled = true;
                resultSlotToolTip.text = new Item(recipe.result).ToolTip();
                resultSlotImage.color = Color.white;
                resultSlotImage.sprite = recipe.result.image;
            } else {
                // refresh invalid recipe
                resultSlotToolTip.enabled = false;
                resultSlotImage.color = Color.clear;
                resultSlotImage.sprite = null;
            }

            // craft button
            craftButton.interactable = recipe != null && player.InventoryCanAddAmount(recipe.result, 1);
            craftButton.onClick.SetListener(() => {
                player.CmdCraft(validIndices.ToArray());
            });
        }
    }
}
