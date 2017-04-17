// Colors the name overlay in case of offender/murderer status.
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class PlayerNameColor : MonoBehaviour {
    [SerializeField] Player owner;
    [SerializeField] Color defaultColor = Color.white;
    [SerializeField] Color offenderColor = Color.magenta;
    [SerializeField] Color murdererColor = Color.red;

    void Update() {
        // note: murderer has higher priority (a player can be a murderer and an
        // offender at the same time)
        if (owner.IsMurderer())
            GetComponent<TextMesh>().color = murdererColor;
        else if (owner.IsOffender())
            GetComponent<TextMesh>().color = offenderColor;
        else
            GetComponent<TextMesh>().color = defaultColor;
    }
}
