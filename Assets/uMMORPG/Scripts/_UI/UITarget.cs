// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class UITarget : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Slider healthSlider;
    [SerializeField] Text nameText;
    [SerializeField] Button tradeButton;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        if (player.target != null && player.target != player) {
            // name and health
            panel.SetActive(true);
            healthSlider.value = player.target.HealthPercent();
            nameText.text = player.target.name;

            // trade button
            if (player.target is Player) {
                tradeButton.gameObject.SetActive(true);
                tradeButton.interactable = player.CanStartTradeWith(player.target);
                tradeButton.onClick.SetListener(() => {
                    player.CmdTradeRequestSend();
                });
            } else tradeButton.gameObject.SetActive(false);
        } else panel.SetActive(false); // hide
    }
}
