using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class UIUtils {
    // instantiate/remove enough prefabs to match amount
    public static void BalancePrefabs(GameObject prefab, int amount, Transform parent) {
        // instantiate until amount
        for (int i = parent.childCount; i < amount; ++i) {
            var go = GameObject.Instantiate(prefab);
            go.transform.SetParent(parent, false);
        }

        // delete everything that's too much
        // (backwards loop because Destroy changes childCount)
        for (int i = parent.childCount-1; i >= amount; --i)
            GameObject.Destroy(parent.GetChild(i).gameObject);
    }

    // find out if any input is currently active by using Selectable.all
    // (FindObjectsOfType<InputField>() is far too slow for huge scenes)
    public static bool AnyInputActive() {
        return Selectable.allSelectables.Any(
            sel => sel is InputField && ((InputField)sel).isFocused
        );
    }

    // deselect any UI element carefully
    // (it throws an error when doing it while clicking somewhere, so we have to
    //  double check)
    public static void DeselectCarefully() {
        if (!Input.GetMouseButton(0) &&
            !Input.GetMouseButton(1) &&
            !Input.GetMouseButton(2))
            EventSystem.current.SetSelectedGameObject(null);
    }
}
