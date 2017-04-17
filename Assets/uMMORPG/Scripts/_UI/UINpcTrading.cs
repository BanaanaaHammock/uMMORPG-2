using UnityEngine;
using UnityEngine.UI;

public class UINpcTrading : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] UINpcTradingSlot slotPrefab;
    [SerializeField] Transform content;
    [SerializeField] UIDragAndDropable buySlot;
    [SerializeField] InputField buyAmountInput;
    [SerializeField] Text buyCostsText;
    [SerializeField] Button buyButton;
    [SerializeField] UIDragAndDropable sellSlot;
    public InputField sellAmountInput;
    [SerializeField] Text sellCostsText;
    [SerializeField] Button sellButton;
    int buyIndex = -1;
    [HideInInspector] public int sellIndex = -1;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // use collider point(s) to also work with big entities
        if (player.target != null && player.target is Npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange) {
            var npc = (Npc)player.target;

            // items for sale
            UIUtils.BalancePrefabs(slotPrefab.gameObject, npc.saleItems.Length, content);
            for (int i = 0; i < npc.saleItems.Length; ++i) {
                var slot = content.GetChild(i).GetComponent<UINpcTradingSlot>();
                var item = npc.saleItems[i];

                // show item in UI
                int icopy = i;
                slot.button.onClick.SetListener(() => {
                    buyIndex = icopy;
                });
                slot.image.color = Color.white;
                slot.image.sprite = item.image;
                slot.tooltip.enabled = true;
                slot.tooltip.text = new Item(item).ToolTip();
            }

            // buy
            if (buyIndex != -1 && buyIndex < npc.saleItems.Length) {
                var item = npc.saleItems[buyIndex];

                // make valid amount, calculate price
                int amount = buyAmountInput.text.ToInt();
                amount = Mathf.Clamp(amount, 1, item.maxStack);
                long price = amount * item.buyPrice;

                // show buy panel with item in UI
                buyAmountInput.text = amount.ToString();
                buySlot.GetComponent<Image>().color = Color.white;
                buySlot.GetComponent<Image>().sprite = item.image;
                buySlot.GetComponent<UIShowToolTip>().enabled = true;
                buySlot.GetComponent<UIShowToolTip>().text = new Item(item).ToolTip();
                buyCostsText.text = price.ToString();
                buyButton.interactable = amount > 0 && price <= player.gold &&
                                         player.InventoryCanAddAmount(item, amount);
                buyButton.onClick.SetListener(() => {
                    player.CmdNpcBuyItem(buyIndex, amount);
                    buyIndex = -1;
                    buyAmountInput.text = "1";
                });
            } else {
                // show default buy panel in UI
                buySlot.GetComponent<Image>().color = Color.clear;
                buySlot.GetComponent<Image>().sprite = null;
                buySlot.GetComponent<UIShowToolTip>().enabled = false;
                buyCostsText.text = "0";
                buyButton.interactable = false;
            }

            // sell
            if (sellIndex != -1 && sellIndex < player.inventory.Count &&
                player.inventory[sellIndex].valid) {
                var item = player.inventory[sellIndex];

                // make valid amount, calculate price
                int amount = sellAmountInput.text.ToInt();
                amount = Mathf.Clamp(amount, 1, item.amount);
                long price = amount * item.sellPrice;

                // show sell panel with item in UI
                sellAmountInput.text = amount.ToString();
                sellSlot.GetComponent<Image>().color = Color.white;
                sellSlot.GetComponent<Image>().sprite = item.image;
                sellSlot.GetComponent<UIShowToolTip>().enabled = true;
                sellSlot.GetComponent<UIShowToolTip>().text = item.ToolTip();
                sellCostsText.text = price.ToString();
                sellButton.interactable = amount > 0;
                sellButton.onClick.SetListener(() => {
                    player.CmdNpcSellItem(sellIndex, amount);
                    sellIndex = -1;
                    sellAmountInput.text = "1";
                });
            } else {
                // show default sell panel in UI
                sellSlot.GetComponent<Image>().color = Color.clear;
                sellSlot.GetComponent<Image>().sprite = null;
                sellSlot.GetComponent<UIShowToolTip>().enabled = false;
                sellCostsText.text = "0";
                sellButton.interactable = false;
            }
        } else panel.SetActive(false); // hide
    }
}
