// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public class UIPlayerTradeRequest : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Text nameText;
    [SerializeField] Button acceptButton;
    [SerializeField] Button declineButton;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // only if there is a request and if not accepted already
        if (player.tradeRequestFrom != "" && player.state != "TRADING") {
            panel.SetActive(true);
            nameText.text = player.tradeRequestFrom;
            acceptButton.onClick.SetListener(() => {
                player.CmdTradeRequestAccept();
            });
            declineButton.onClick.SetListener(() => {
                player.CmdTradeRequestDecline();
            });
        } else panel.SetActive(false); // hide
    }
}
