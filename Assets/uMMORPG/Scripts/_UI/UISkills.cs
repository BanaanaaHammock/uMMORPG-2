// Note: this script has to be on an always-active UI parent, so that we can
// always react to the hotkey.
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Serialization;

public class UISkills : MonoBehaviour {
    [SerializeField] KeyCode hotKey = KeyCode.R;
    [SerializeField] GameObject panel;
    [SerializeField] UISkillSlot slotPrefab;
    [SerializeField] Transform content;
    [SerializeField] Text skillExperienceText;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // hotkey (not while typing in chat, etc.)
        if (Input.GetKeyDown(hotKey) && !UIUtils.AnyInputActive())
            panel.SetActive(!panel.activeSelf);

        // only update the panel if it's active
        if (panel.activeSelf) {
            // instantiate/destroy enough slots
            // (we only care about non status skills)
            var skills = player.skills.Where(s => !s.category.StartsWith("Status")).ToList();
            UIUtils.BalancePrefabs(slotPrefab.gameObject, skills.Count, content);

            // refresh all
            for (int i = 0; i < skills.Count; ++i) {
                var slot = content.GetChild(i).GetComponent<UISkillSlot>();
                var skill = skills[i];
                
                // drag and drop name has to be the index in the real skill list,
                // not in the filtered list, otherwise drag and drop may fail
                int skillIndex = player.skills.FindIndex(s => s.name == skill.name);
                slot.dragAndDropable.name = skillIndex.ToString();
                
                // click event
                slot.button.interactable = skill.learned;
                slot.button.onClick.SetListener(() => {
                    if (skill.learned && skill.IsReady()) 
                        player.CmdUseSkill(skillIndex);
                });
                
                // set state
                slot.dragAndDropable.dragable = skill.learned;

                // image
                if (skill.learned) {
                    slot.image.color = Color.white;
                    slot.image.sprite = skill.image;
                }

                // description
                slot.descriptionText.text = skill.ToolTip(showRequirements: !skill.learned);

                // learnable?
                if (!skill.learned) {
                    slot.learnButton.gameObject.SetActive(true);
                    slot.learnButton.GetComponentInChildren<Text>().text = "Learn";
                    slot.learnButton.interactable = player.level >= skill.requiredLevel &&
                                                    player.skillExperience >= skill.requiredSkillExperience;
                    slot.learnButton.onClick.SetListener(() => { player.CmdLearnSkill(skillIndex); });
                // upgradeable?
                } else if (skill.level < skill.maxLevel) {
                    slot.learnButton.gameObject.SetActive(true);
                    slot.learnButton.GetComponentInChildren<Text>().text = "Upgrade";
                    slot.learnButton.interactable = player.level >= skill.upgradeRequiredLevel &&
                                                    player.skillExperience >= skill.upgradeRequiredSkillExperience;
                    slot.learnButton.onClick.SetListener(() => { player.CmdUpgradeSkill(skillIndex); });
                // otherwise no button needed
                } else slot.learnButton.gameObject.SetActive(false);

                // cooldown overlay
                float cooldown = skill.CooldownRemaining();
                slot.cooldownOverlay.SetActive(skill.learned && cooldown > 0);
                slot.cooldownText.text = cooldown.ToString("F0");
            }

            // skill experience
            skillExperienceText.text = player.skillExperience.ToString();
        }
    }
}
