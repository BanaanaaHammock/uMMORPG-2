using UnityEngine;
using System.Linq;

[RequireComponent(typeof(TextMesh))]
public class NpcQuestIndicator : MonoBehaviour {
    [SerializeField] Npc owner;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // can complete = !; can start = ?; nothing = ""
        if (owner.quests.Any(q => player.CanCompleteQuest(q.name)))
            GetComponent<TextMesh>().text = "!";
        else if (owner.quests.Any(q => player.CanStartQuest(q)))
            GetComponent<TextMesh>().text = "?";
        else
            GetComponent<TextMesh>().text = "";
    }
}
