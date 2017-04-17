// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;

public class UIRespawn : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Button button;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // visible while player is dead
        panel.SetActive(player.health == 0);
        button.onClick.SetListener(() => { player.CmdRespawn(); });
    }
}
