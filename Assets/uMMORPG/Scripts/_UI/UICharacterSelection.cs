// Simple character selection list. The charcter prefabs are known, so we could
// easily show 3D models, stats, etc. too.
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;

public class UICharacterSelection : MonoBehaviour {
    [SerializeField] NetworkManagerMMO manager; // singleton is null until update
    [SerializeField] GameObject panel;
    [SerializeField] UICharacterSelectionSlot slotPrefab;
    [SerializeField] Transform content;
    // available characters (set after receiving the message from the server)
    [HideInInspector] public CharactersAvailableMsg characters;
    [SerializeField] Button createButton;
    [SerializeField] Button quitButton;

    void Update() {
        // only update if visible
        if (!panel.activeSelf) return;

        // hide if disconnected or if a local player is in the game world
        if (!NetworkClient.active || Utils.ClientLocalPlayer() != null) Hide();

        // instantiate/destroy enough slots
        UIUtils.BalancePrefabs(slotPrefab.gameObject, characters.characterNames.Length, content);

        // refresh all
        var prefabs = manager.GetPlayerClasses();
        for (int i = 0; i < characters.characterNames.Length; ++i) {
            var prefab = prefabs.Find(p => p.name == characters.characterClasses[i]);
            var slot = content.GetChild(i).GetComponent<UICharacterSelectionSlot>();

            // name and icon
            slot.nameText.text = characters.characterNames[i];
            slot.image.sprite = prefab.GetComponent<Player>().classIcon;
            
            // select button: calls AddPLayer which calls OnServerAddPlayer
            int icopy = i; // needed for lambdas, otherwise i is Count
            slot.selectButton.onClick.SetListener(() => {
                var message = new CharacterSelectMsg{index=icopy};
                ClientScene.AddPlayer(manager.client.connection, 0, message);
            });
            
            // delete button: sends delete message
            slot.deleteButton.onClick.SetListener(() => {
                var message = new CharacterDeleteMsg{index=icopy};
                manager.client.Send(CharacterDeleteMsg.MsgId, message);
            });
        }
        
        createButton.interactable = characters.characterNames.Length < manager.characterLimit;
        createButton.onClick.SetListener(() => {
            Hide();
            FindObjectOfType<UICharacterCreation>().Show();
        });
        
        quitButton.onClick.SetListener(() => { Application.Quit(); });
    }

    public void Hide() { panel.SetActive(false); }
    public void Show() { panel.SetActive(true); }
}
