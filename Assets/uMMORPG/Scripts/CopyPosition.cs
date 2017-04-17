// This component copies a Transform's position to automatically follow it,
// which is useful for the camera.
using UnityEngine;

public class CopyPosition : MonoBehaviour {
    public bool x, y, z = false;
    public Transform target = null;

    void Update () {
        if (target)
            transform.position = new Vector3(
                (x ? target.position.x : transform.position.x),
                (y ? target.position.y : transform.position.y),
                (z ? target.position.z : transform.position.z));
    }
}
