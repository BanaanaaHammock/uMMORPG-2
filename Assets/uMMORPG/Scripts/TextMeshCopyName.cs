// Copies a GameObject's name to the TextMesh's text. We use the Update method
// because some GameObjects may change their name during the game, like players
// renaming themself etc.
using UnityEngine;

[RequireComponent(typeof(TextMesh))]
public class TextMeshCopyName : MonoBehaviour {
    [SerializeField] GameObject source;

    void Update() {
        GetComponent<TextMesh>().text = source.name;
    }
}
