// Note: this script has to be on an always-active UI parent, so that we can
// always find it from other code. (GameObject.Find doesn't find inactive ones)
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using System.Linq;

public class UILogin : MonoBehaviour {
    [SerializeField] NetworkManagerMMO manager; // singleton=null in Start/Awake
    [SerializeField] GameObject panel;
    [SerializeField] Text statusText;
    [SerializeField] InputField accountInput;
    [SerializeField] InputField passwordInput;
    [SerializeField] Dropdown serverDropdown;
    [SerializeField] Button loginButton;
    [SerializeField] Button registerButton;
    [SerializeField, TextArea(1, 30)] string registerMessage = "First time? Just log in and we will\ncreate an account automatically.";
    [SerializeField] Button hostButton;
    [SerializeField] Button dedicatedButton;
    [SerializeField] Button cancelButton;
    [SerializeField] Button quitButton;

    void Start() {
        // load last server by name in case order changes some day.
        if (PlayerPrefs.HasKey("LastServer")) {
            string last = PlayerPrefs.GetString("LastServer", "");
            serverDropdown.value = manager.serverList.FindIndex(s => s.name == last);
        }
    }

    void OnDestroy() {
        // save last server by name in case order changes some day
        PlayerPrefs.SetString("LastServer", serverDropdown.captionText.text);
    }

    void Update() {
        // only update while visible
        if (!panel.activeSelf) return;
        
        // status
        statusText.text = manager.IsConnecting() ? "Connecting..." : "";

        // buttons
        registerButton.onClick.SetListener(() => { FindObjectOfType<UIPopup>().Show(registerMessage); });
        loginButton.interactable = !manager.IsConnecting();
        loginButton.onClick.SetListener(() => { manager.StartClient(); });
        hostButton.interactable = !manager.IsConnecting();
        hostButton.onClick.SetListener(() => { manager.StartHost(); });
        cancelButton.gameObject.SetActive(manager.IsConnecting());
        cancelButton.onClick.SetListener(() => { manager.StopClient(); });
        dedicatedButton.onClick.SetListener(() => { manager.StartServer(); });
        dedicatedButton.interactable = !manager.IsConnecting();
        quitButton.onClick.SetListener(() => { Application.Quit(); });

        // inputs
        manager.id = accountInput.text;
        manager.pw = passwordInput.text;

        // copy servers to dropdown; copy selected one to networkmanager ip/port.
        serverDropdown.interactable = !manager.IsConnecting();
        serverDropdown.options = manager.serverList.Select(
            sv => new Dropdown.OptionData(sv.name)
        ).ToList();
        manager.networkAddress = manager.serverList[serverDropdown.value].ip;
    }

    public void Show() { panel.SetActive(true); }
    public void Hide() { panel.SetActive(false); }
}
