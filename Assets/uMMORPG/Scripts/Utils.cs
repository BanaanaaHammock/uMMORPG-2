// This class contains some helper functions.
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Networking;
using UnityEngine.EventSystems;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Security.Cryptography;

public class Utils {
    // Mathf.Clamp only works for float and int. we need some more versions:
    public static long Clamp(long value, long min, long max) {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
    
    // is any of the keys UP?
    public static bool AnyKeyUp(KeyCode[] keys) {
        return keys.Any(k => Input.GetKeyUp(k));
    }

    // is any of the keys DOWN?
    public static bool AnyKeyDown(KeyCode[] keys) {
        return keys.Any(k => Input.GetKeyDown(k));
    }

    // detect headless mode (which has graphicsDeviceType Null)
    public static bool IsHeadless() {
        return SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    }

    // String.IsNullOrWhiteSpace that exists in NET4.5
    // note: can't be an extension because then it can't detect null strings
    //       like null.IsNullOrWhitespace
    public static bool IsNullOrWhiteSpace(string value) {
        return System.String.IsNullOrEmpty(value) || value.Trim().Length == 0;
    }

    // Distance between two ClosestPointOnBounds
    // this is needed in cases where entites are really big. in those cases,
    // we can't just move to entity.transform.position, because it will be
    // unreachable. instead we have to go the closest point on the boundary.
    //
    // Vector3.Distance(a.transform.position, b.transform.position):
    //    _____        _____
    //   |     |      |     |
    //   |  x==|======|==x  |
    //   |_____|      |_____|
    //
    //
    // Utils.ClosestDistance(a.collider, b.collider):
    //    _____        _____
    //   |     |      |     |
    //   |     |x====x|     |
    //   |_____|      |_____|
    //  
    public static float ClosestDistance(Collider a, Collider b) {
        return Vector3.Distance(a.ClosestPointOnBounds(b.transform.position),
                                b.ClosestPointOnBounds(a.transform.position));
    }

    // pretty print seconds as hours:minutes:seconds(.milliseconds/100)s
    public static string PrettySeconds(float seconds) {
        var t = System.TimeSpan.FromSeconds(seconds);
        string res = "";
        if (t.Days > 0) res += t.Days + "d";
        if (t.Hours > 0) res += " " + t.Hours + "h";
        if (t.Minutes > 0) res += " " + t.Minutes + "m";
        // 0.5s, 1.5s etc. if any milliseconds. 1s, 2s etc. if any seconds
        if (t.Milliseconds > 0) res += " " + t.Seconds + "." + (t.Milliseconds / 100) + "s";
        else if (t.Seconds > 0) res += " " + t.Seconds + "s";
        // if the string is still empty because the value was '0', then at least
        // return the seconds instead of returning an empty string
        return res != "" ? res : "0s";
    }

    // hard mouse scrolling that is consistent between all platforms
    //   Input.GetAxis("Mouse ScrollWheel") and
    //   Input.GetAxisRaw("Mouse ScrollWheel")
    //   both return values like 0.01 on standalone and 0.5 on WebGL, which
    //   causes too fast zooming on WebGL etc.
    // normally GetAxisRaw should return -1,0,1, but it doesn't for scrolling
    public static float GetAxisRawScrollUniversal() {
        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll < 0) return -1;
        if (scroll > 0) return  1;
        return 0;
    }

    // two finger pinch detection
    // source: https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
    public static float GetPinch() {
        if (Input.touchCount == 2) {
            // Store both touches.
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Find the position in the previous frame of each touch.
            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            // Find the magnitude of the vector (the distance) between the touches in each frame.
            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            // Find the difference in the distances between each frame.
            return touchDeltaMag - prevTouchDeltaMag;
        }
        return 0;
    }

    // universal zoom: mouse scroll if mouse, two finger pinching otherwise
    public static float GetZoomUniversal() {
        if (Input.mousePresent)
            return Utils.GetAxisRawScrollUniversal();
        else if (Input.touchSupported)
            return GetPinch();
        return 0;
    }

    // find local player (clientsided)
    public static Player ClientLocalPlayer() {
        // note: ClientScene.localPlayers.Count cant be used as check because
        // nothing is removed from that list, even after disconnect. It still
        // contains entries like: ID=0 NetworkIdentity NetID=null Player=null
        // (which might be a UNET bug)
        var p = ClientScene.localPlayers.Find(pc => pc.gameObject != null);
        return p != null ? p.gameObject.GetComponent<Player>() : null;
    }

    // parse first upper cased noun from a string, e.g.
    //   EquipmentWeaponBow => Equipment
    //   EquipmentShield => Equipment
    public static string ParseFirstNoun(string text) {
        var matches = new Regex(@"([A-Z][a-z]*)").Matches(text);
        return matches.Count > 0 ? matches[0].Value : "";
    }

    // parse last upper cased noun from a string, e.g.
    //   EquipmentWeaponBow => Bow
    //   EquipmentShield => Shield
    public static string ParseLastNoun(string text) {
        var matches = new Regex(@"([A-Z][a-z]*)").Matches(text);
        return matches.Count > 0 ? matches[matches.Count-1].Value : "";
    }

    // check if the cursor is over a UI or OnGUI element right now
    // note: for UI, this only works if the UI's CanvasGroup blocks Raycasts
    // note: for OnGUI: hotControl is only set while clicking, not while zooming
    public static bool IsCursorOverUserInterface() {
        // IsPointerOverGameObject check for left mouse (default)
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // IsPointerOverGameObject check for touches
        for (int i = 0; i < Input.touchCount; ++i)
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;

        // OnGUI check
        return GUIUtility.hotControl != 0;
    }

    // PBKDF2 hashing recommended by NIST:
    // http://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-132.pdf
    // salt should be at least 128 bits = 16 bytes
    public static string PBKDF2Hash(string text, string salt) {
        var saltBytes = Encoding.UTF8.GetBytes(salt);
        var pbkdf2 = new Rfc2898DeriveBytes(text, saltBytes, 10000);
        byte[] hash = pbkdf2.GetBytes(20);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}
