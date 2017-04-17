using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class UINpcQuests : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] UINpcQuestSlot slotPrefab;
    [SerializeField] Transform content;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // npc quest
        // use collider point(s) to also work with big entities
        if (player.target != null && player.target is Npc &&
            Utils.ClosestDistance(player.collider, player.target.collider) <= player.interactionRange) {
            var npc = (Npc)player.target;

            // instantiate/destroy enough slots
            var questsAvailable = npc.QuestsVisibleFor(player);
            UIUtils.BalancePrefabs(slotPrefab.gameObject, questsAvailable.Count, content);

            // refresh all
            for (int i = 0; i < questsAvailable.Count; ++i) {
                var slot = content.GetChild(i).GetComponent<UINpcQuestSlot>();

                // find quest index in original npc quest list (unfiltered)
                int npcIndex = Array.FindIndex(npc.quests, q => q.name == questsAvailable[i].name);

                // find quest index in player quest list
                int questIndex = player.GetQuestIndexByName(npc.quests[npcIndex].name);
                if (questIndex != -1) {
                    // running quest: shows description with current progress
                    // instead of static one
                    var quest = player.quests[questIndex];
                    var reward = npc.quests[npcIndex].rewardItem;
                    int gathered = quest.gatherName != "" ? player.InventoryCountAmount(quest.gatherName) : 0;
                    bool hasSpace = reward == null || player.InventoryCanAddAmount(reward, 1);
                    
                    slot.descriptionText.text = quest.ToolTip(gathered);
                    if (!hasSpace)
                        slot.descriptionText.text += "\n<color=red>Not enough inventory space!</color>";
                    
                    slot.actionButton.interactable = quest.IsFulfilled(gathered) && hasSpace;
                    slot.actionButton.GetComponentInChildren<Text>().text = "Complete";
                    slot.actionButton.onClick.SetListener(() => {
                        player.CmdCompleteQuest(npcIndex);
                        panel.SetActive(false);
                    });
                } else {
                    // new quest
                    slot.descriptionText.text = new Quest(npc.quests[npcIndex]).ToolTip();
                    slot.actionButton.interactable = true;
                    slot.actionButton.GetComponentInChildren<Text>().text = "Accept";
                    slot.actionButton.onClick.SetListener(() => {
                        player.CmdAcceptQuest(npcIndex);
                    });
                }
            }
        } else panel.SetActive(false); // hide
    }
}
