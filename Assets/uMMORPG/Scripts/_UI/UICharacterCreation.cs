using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

public class UICharacterCreation : MonoBehaviour {
    [SerializeField] NetworkManagerMMO manager; // singleton is null until update
    [SerializeField] GameObject panel;
    [SerializeField] InputField nameInput;
    [SerializeField] Dropdown classDropdown;
    [SerializeField] Button createButton;
    [SerializeField] Button cancelButton;

    void Update() {
        // only update while visible
        if (!panel.activeSelf) return;

        // hide if disconnected
        if (!NetworkClient.active) Hide();

        // copy player classes to class selection
        classDropdown.options = manager.GetPlayerClasses().Select(
            p => new Dropdown.OptionData(p.name)
        ).ToList();

        // buttons
        createButton.onClick.SetListener(() => {
            var message = new CharacterCreateMsg{
                name = nameInput.text,
                classIndex = classDropdown.value
            };
            manager.client.Send(CharacterCreateMsg.MsgId, message);
        });        
        cancelButton.onClick.SetListener(() => {
            nameInput.text = "";
            Hide();
            FindObjectOfType<UICharacterSelection>().Show();
        });
    }

    public void Hide() { panel.SetActive(false); }
    public void Show() { panel.SetActive(true); }
}
