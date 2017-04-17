using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIMinimap : MonoBehaviour {
    [SerializeField] GameObject panel;
    [SerializeField] float zoomMin = 5;
    [SerializeField] float zoomMax = 50;
    [SerializeField] float zoomStepSize = 5;
    [SerializeField] Text sceneText;
    [SerializeField] Button plusButton;
    [SerializeField] Button minusButton;
    [SerializeField] Camera minimapCamera;

    void Start() {
        plusButton.onClick.SetListener(() => {
            minimapCamera.orthographicSize = Mathf.Max(minimapCamera.orthographicSize - zoomStepSize, zoomMin);
        });
        minusButton.onClick.SetListener(() => {
            minimapCamera.orthographicSize = Mathf.Min(minimapCamera.orthographicSize + zoomStepSize, zoomMax);
        });
    }

    void Update() {
        var player = Utils.ClientLocalPlayer();
        panel.SetActive(player != null); // hide while not in the game world
        if (!player) return;

        sceneText.text = SceneManager.GetActiveScene().name;
    }
}
