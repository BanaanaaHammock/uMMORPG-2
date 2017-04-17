// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class UINpcDialogue : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Text welcomeText;
    [SerializeField] Button tradingButton;
    [SerializeField] Button teleportButton;
    [SerializeField] Button questsButton;
    [SerializeField] GameObject npcTradingPanel;
    [SerializeField] GameObject npcQuestPanel;
    [SerializeField] GameObject inventoryPanel;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // use collider point(s) to also work with big entities
        if (panel.activeSelf &&
            player.target != null && player.target is Npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange) {
            var npc = (Npc)player.target;
            
            // welcome text
            welcomeText.text = npc.welcome;

            // trading button
            tradingButton.gameObject.SetActive(npc.saleItems.Length > 0);
            tradingButton.onClick.SetListener(() => {
                npcTradingPanel.SetActive(true);
                inventoryPanel.SetActive(true); // better feedback
                panel.SetActive(false);
            });

            // teleport button
            teleportButton.gameObject.SetActive(npc.teleportTo != null);
            if (npc.teleportTo != null)
                teleportButton.GetComponentInChildren<Text>().text = "Teleport: " + npc.teleportTo.name;
            teleportButton.onClick.SetListener(() => {
                player.CmdNpcTeleport();
            });

            // filter out the quests that are available for the player
            var questsAvailable = npc.QuestsVisibleFor(player);
            questsButton.gameObject.SetActive(questsAvailable.Count > 0);
            questsButton.onClick.SetListener(() => {
                npcQuestPanel.SetActive(true);
                panel.SetActive(false);
            });
        } else panel.SetActive(false); // hide
    }

    public void Show() { panel.SetActive(true); }
}
