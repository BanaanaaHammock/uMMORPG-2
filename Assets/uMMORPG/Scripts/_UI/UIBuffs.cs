using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class UIBuffs : MonoBehaviour {
    [SerializeField] UIBuffSlot slotPrefab;

    void Update() {
        var player = Utils.ClientLocalPlayer();
        if (!player) return;

        // instantiate/destroy enough slots
        var buffs = player.skills.Where(s => s.BuffTimeRemaining() > 0).ToList();
        UIUtils.BalancePrefabs(slotPrefab.gameObject, buffs.Count, transform);

        // refresh all
        for (int i = 0; i < buffs.Count; ++i) {
            var slot = transform.GetChild(i).GetComponent<UIBuffSlot>();

            // refresh
            slot.image.color = Color.white;
            slot.image.sprite = buffs[i].image;
            slot.tooltip.text = buffs[i].ToolTip();
            slot.slider.maxValue = buffs[i].buffTime;
            slot.slider.value = buffs[i].BuffTimeRemaining();
        }
    }
}