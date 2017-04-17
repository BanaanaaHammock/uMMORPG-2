using UnityEngine;
using UnityEngine.UI;

public class UITextCopyName : MonoBehaviour {
    public GameObject source;

    void Update() {
        GetComponent<Text>().text = source.name;
    }
}
