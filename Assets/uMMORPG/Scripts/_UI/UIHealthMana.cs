using UnityEngine;
using UnityEngine.UI;

public class UIHealthMana : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Slider healthSlider;
    [SerializeField] Text healthStatus;
    [SerializeField] Slider manaSlider;
    [SerializeField] Text manaStatus;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        healthSlider.value = player.HealthPercent();
        healthStatus.text = player.health + " / " + player.healthMax;

        manaSlider.value = player.ManaPercent();
        manaStatus.text = player.mana + " / " + player.manaMax;
    }
}
