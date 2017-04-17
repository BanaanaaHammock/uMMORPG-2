// Takes care of Drag and Drop events for the player.
// Works with UI and OnGUI. Simply do:
//   FindObjectOfType<PlayerDndHandling>().OnDragAndDrop(...);
using UnityEngine;

[RequireComponent(typeof(Player))]
public class PlayerDndHandling : MonoBehaviour {
    // cache
    Player player;

    // remove self if not local player
    void Start() {
        player = GetComponent<Player>();
        if (!player.isLocalPlayer) Destroy(this);
    }

    public void OnDragAndDrop(string fromType, int from, string toType, int to) {
        // call OnDnd_From_To dynamically
        print("OnDragAndDrop from: " + fromType + " " + from + " to: " + toType + " " + to);
        SendMessage("OnDnd_" + fromType + "_" + toType, new int[]{from, to},
                    SendMessageOptions.DontRequireReceiver);
    }

    public void OnDragAndClear(string type, int from) {
        // clear it for some slot types
        if (type == "SkillbarSlot") player.skillbar[from] = "";
        if (type == "TradingSlot") player.CmdTradeOfferItemClear(from);
        if (type == "NpcSellSlot") FindObjectOfType<UINpcTrading>().sellIndex = -1;
        if (type == "CraftingIngredientSlot") player.craftingIndices[from] = -1;
    }

    ////////////////////////////////////////////////////////////////////////////
    void OnDnd_InventorySlot_InventorySlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo

        // merge? (just check the name, rest is done server sided)
        if (player.inventory[slotIndices[0]].valid && player.inventory[slotIndices[1]].valid &&
            player.inventory[slotIndices[0]].name == player.inventory[slotIndices[1]].name) {
            player.CmdInventoryMerge(slotIndices[0], slotIndices[1]);
        // split?
        } else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) {
            player.CmdInventorySplit(slotIndices[0], slotIndices[1]);
        // swap?
        } else {
            player.CmdSwapInventoryInventory(slotIndices[0], slotIndices[1]);
        }
    }
    
    void OnDnd_InventorySlot_TrashSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        player.CmdSwapInventoryTrash(slotIndices[0]);
    }

    void OnDnd_InventorySlot_EquipmentSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        player.CmdSwapInventoryEquip(slotIndices[0], slotIndices[1]);
    }

    void OnDnd_InventorySlot_SkillbarSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        player.skillbar[slotIndices[1]] = player.inventory[slotIndices[0]].name; // just save it clientsided
    }

    void OnDnd_InventorySlot_NpcSellSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (player.inventory[slotIndices[0]].sellable) {
            FindObjectOfType<UINpcTrading>().sellIndex = slotIndices[0];
            FindObjectOfType<UINpcTrading>().sellAmountInput.text = player.inventory[slotIndices[0]].amount.ToString();
        }
    }

    void OnDnd_InventorySlot_TradingSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (player.inventory[slotIndices[0]].tradable)
            player.CmdTradeOfferItem(slotIndices[0], slotIndices[1]);
    }

    void OnDnd_InventorySlot_CraftingIngredientSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        if (!player.craftingIndices.Contains(slotIndices[0]))
            player.craftingIndices[slotIndices[1]] = slotIndices[0];
    }

    void OnDnd_TrashSlot_InventorySlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        player.CmdSwapTrashInventory(slotIndices[1]);
    }

    void OnDnd_EquipmentSlot_InventorySlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        player.CmdSwapInventoryEquip(slotIndices[1], slotIndices[0]); // reversed
    }

    void OnDnd_EquipmentSlot_SkillbarSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        player.skillbar[slotIndices[1]] = player.equipment[slotIndices[0]].name; // just save it clientsided
    }

    void OnDnd_SkillsSlot_SkillbarSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        player.skillbar[slotIndices[1]] = player.skills[slotIndices[0]].name; // just save it clientsided
    }

    void OnDnd_SkillbarSlot_SkillbarSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // just swap them clientsided
        var temp = player.skillbar[slotIndices[0]];
        player.skillbar[slotIndices[0]] = player.skillbar[slotIndices[1]];
        player.skillbar[slotIndices[1]] = temp;
    }

    void OnDnd_CraftingIngredientSlot_CraftingIngredientSlot(int[] slotIndices) {
        // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
        // just swap them clientsided
        var temp = player.craftingIndices[slotIndices[0]];
        player.craftingIndices[slotIndices[0]] = player.craftingIndices[slotIndices[1]];
        player.craftingIndices[slotIndices[1]] = temp;
    }
}
