// This component can be attached to moveable windows, so that they are only
// moveable within the Screen boundaries.
using UnityEngine;

public class UIKeepInScreen : MonoBehaviour {
    void Update () {
        // get current rectangle
        var rect = GetComponent<RectTransform>().rect;
            
        // to world space
        Vector2 minworld = transform.TransformPoint(rect.min);
        Vector2 maxworld = transform.TransformPoint(rect.max);
        var sizeworld = maxworld - minworld;
        
        // keep the min position in screen bounds - size
        maxworld = new Vector2(Screen.width, Screen.height) - sizeworld;
        
        // keep position between (0,0) and maxworld
        float x = Mathf.Clamp(minworld.x, 0, maxworld.x);
        float y = Mathf.Clamp(minworld.y, 0, maxworld.y);
        
        // set new position to xy(=local) + offset(=world)
        Vector2 offset = (Vector2)transform.position - minworld;
        transform.position = new Vector2(x, y) + offset;
    }
}
