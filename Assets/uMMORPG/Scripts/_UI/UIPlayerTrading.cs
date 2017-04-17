// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerTrading : MonoBehaviour {
    [SerializeField] GameObject panel;

    [SerializeField] UIPlayerTradingSlot slotPrefab;

    [SerializeField] Transform otherContent;
    [SerializeField] Text otherStatusText;
    [SerializeField] InputField otherGoldInput;

    [SerializeField] Transform myContent;
    [SerializeField] Text myStatusText;
    [SerializeField] InputField myGoldInput;

    [SerializeField] Button lockButton;
    [SerializeField] Button acceptButton;
    [SerializeField] Button cancelButton;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // only if trading, otherwise set inactive
        if (player.state == "TRADING" && player.target != null && player.target is Player) {
            panel.SetActive(true);
            var other = (Player)player.target;

            // OTHER ///////////////////////////////////////////////////////////
            // status text
            if (other.tradeOfferAccepted) otherStatusText.text = "[ACCEPTED]";
            else if (other.tradeOfferLocked) otherStatusText.text = "[LOCKED]";
            else otherStatusText.text = "";
            
            // gold input
            otherGoldInput.text = other.tradeOfferGold.ToString();
            
            // items
            UIUtils.BalancePrefabs(slotPrefab.gameObject, other.tradeOfferItems.Count, otherContent);
            for (int i = 0; i < other.tradeOfferItems.Count; ++i) {
                var slot = otherContent.GetChild(i).GetComponent<UIPlayerTradingSlot>();
                int inventoryIndex = other.tradeOfferItems[i];

                slot.dragAndDropable.dragable = false;
                slot.dragAndDropable.dropable = false;

                if (0 <= inventoryIndex && inventoryIndex < other.inventory.Count &&
                    other.inventory[inventoryIndex].valid) {
                    var item = other.inventory[inventoryIndex];

                    // refresh valid item
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = item.ToolTip();
                    slot.image.color = Color.white;
                    slot.image.sprite = item.image;
                    slot.amountOverlay.SetActive(item.amount > 1);
                    slot.amountText.text = item.amount.ToString();
                } else {
                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }

            // SELF ////////////////////////////////////////////////////////////
            // status text
            if (player.tradeOfferAccepted) myStatusText.text = "[ACCEPTED]";
            else if (player.tradeOfferLocked) myStatusText.text = "[LOCKED]";
            else myStatusText.text = "";
            
            // gold input
            if (player.tradeOfferLocked) {
                myGoldInput.interactable = false;
                myGoldInput.text = player.tradeOfferGold.ToString();
            } else {
                myGoldInput.interactable = true;
                myGoldInput.onValueChanged.SetListener((val) => {
                    long goldOffer = Utils.Clamp(val.ToLong(), 0, player.gold);
                    myGoldInput.text = goldOffer.ToString();
                    player.CmdTradeOfferGold(goldOffer);
                });
            }
            
            // items
            UIUtils.BalancePrefabs(slotPrefab.gameObject, player.tradeOfferItems.Count, myContent);
            for (int i = 0; i < player.tradeOfferItems.Count; ++i) {
                var slot = myContent.GetChild(i).GetComponent<UIPlayerTradingSlot>();
                slot.dragAndDropable.name = i.ToString(); // drag and drop index
                int inventoryIndex = player.tradeOfferItems[i];

                if (0 <= inventoryIndex && inventoryIndex < player.inventory.Count &&
                    player.inventory[inventoryIndex].valid) {
                    var item = player.inventory[inventoryIndex];

                    // refresh valid item
                    slot.tooltip.enabled = true;
                    slot.tooltip.text = item.ToolTip();
                    slot.dragAndDropable.dragable = !player.tradeOfferLocked;
                    slot.image.color = Color.white;
                    slot.image.sprite = item.image;
                    slot.amountOverlay.SetActive(item.amount > 1);
                    slot.amountText.text = item.amount.ToString();
                } else {
                    // refresh invalid item
                    slot.tooltip.enabled = false;
                    slot.dragAndDropable.dragable = false;
                    slot.image.color = Color.clear;
                    slot.image.sprite = null;
                    slot.amountOverlay.SetActive(false);
                }
            }

            // buttons /////////////////////////////////////////////////////////
            // lock
            lockButton.interactable = !player.tradeOfferLocked;
            lockButton.onClick.SetListener(() => {
                player.CmdTradeOfferLock();
            });
            
            // accept (only if both have locked the trade & if not accepted yet)
            acceptButton.interactable = player.tradeOfferLocked && other.tradeOfferLocked && !player.tradeOfferAccepted;
            acceptButton.onClick.SetListener(() => {
                player.CmdTradeOfferAccept();
            });

            // cancel
            cancelButton.onClick.SetListener(() => {
                player.CmdTradeCancel();
            });
        } else {
            panel.SetActive(false);
            myGoldInput.text = "0"; // reset
        }        
    }
}
