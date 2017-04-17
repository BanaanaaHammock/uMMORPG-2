using UnityEngine;
using UnityEngine.UI;

public class UIExperienceBar : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] Slider slider;
    [SerializeField] Text statusText;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        slider.value = player.ExperiencePercent();
        statusText.text = "Lv." + player.level + " (" + (player.ExperiencePercent() * 100).ToString("F2") + "%)";
    }
}
