// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;

public class UICharacterInfo : MonoBehaviour {
    [SerializeField] KeyCode hotKey = KeyCode.C;
    [SerializeField] GameObject panel;
    [SerializeField] Text damageText;
    [SerializeField] Text defenseText;
    [SerializeField] Text healthText;
    [SerializeField] Text manaText;
    [SerializeField] Text speedText;
    [SerializeField] Text levelText;
    [SerializeField] Text currentExperienceText;
    [SerializeField] Text maximumExperienceText;
    [SerializeField] Text skillExperienceText;
    [SerializeField] Text strengthText;
    [SerializeField] Text intelligenceText;
    [SerializeField] Button strengthButton;
    [SerializeField] Button intelligenceButton;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only refresh the panel while it's active
        if (panel.activeSelf) {
            damageText.text = player.damage.ToString();
            defenseText.text = player.defense.ToString();
            healthText.text = player.healthMax.ToString();
            manaText.text = player.manaMax.ToString();
            speedText.text = player.speed.ToString();
            levelText.text = player.level.ToString();
            currentExperienceText.text = player.experience.ToString();
            maximumExperienceText.text = player.experienceMax.ToString();
            skillExperienceText.text = player.skillExperience.ToString();

            strengthText.text = player.strength.ToString();
            strengthButton.interactable = player.AttributesSpendable() > 0;
            strengthButton.onClick.SetListener(() => {
                player.CmdIncreaseStrength();
            });

            intelligenceText.text = player.intelligence.ToString();
            intelligenceButton.interactable = player.AttributesSpendable() > 0;
            intelligenceButton.onClick.SetListener(() => {
                player.CmdIncreaseIntelligence();
            });
        }
    }
}
