// We developed a simple but useful MMORPG style camera. The player can zoom in
// and out with the mouse wheel and rotate the camera around the hero by holding
// down the right mouse button.
//
// Note: we turned off the linecast obstacle detection because it would require
// colliders on all environment models, which means additional physics
// complexity (which is not needed due to navmesh movement) and additional
// components on many gameobjects. Even if performance is not a problem, there
// is still the weird case where if a tent would have a collider, the inside
// would still be part of the navmesh, but it's not clickable because of the 
// collider. Clicking on top of that collider would move the agent into the tent
// though, which is not very good. Not worrying about all those things and
// having a fast server is a better tradeoff.
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraMMO : MonoBehaviour {
    public Transform target;

    public int mouseButton = 1; // right button by default

    public float distance = 20;
    public float minDistance = 3;
    public float maxDistance = 20;

    public float zoomSpeedMouse = 1;
    public float zoomSpeedTouch = 0.2f;
    public float rotationSpeed = 2;

    public float yMinAngle = -40;
    public float yMaxAngle = 80;

    // the target position can be adjusted by an offset in order to foucs on a
    // target's head for example
    public Vector3 offset = Vector3.zero;

    // the layer mask to use when trying to detect view blocking
    // (this way we dont zoom in all the way when standing in another entity)
    // (-> create a entity layer for them if needed)
    //public LayerMask layerMask;

    // store rotation so that unity never modifies it, otherwise unity will put
    // it back to 360 as soon as it's <0, which makes a negative min angle
    // impossible
    Vector3 rot;

    void Awake () {
        rot = transform.eulerAngles;
    }

    void LateUpdate () {
        if (!target) return;

        var targetPos = target.position + offset;

        // rotation and zoom should only happen if not in a UI right now
        if (!Utils.IsCursorOverUserInterface()) {
            // right mouse rotation if we have a mouse
            if (Input.mousePresent) {
                if (Input.GetMouseButton(mouseButton)) {
                    // note: mouse x is for y rotation and vice versa
                    rot.y += Input.GetAxis("Mouse X") * rotationSpeed;
                    rot.x -= Input.GetAxis("Mouse Y") * rotationSpeed;
                    rot.x = Mathf.Clamp(rot.x, yMinAngle, yMaxAngle);
                    transform.rotation = Quaternion.Euler(rot.x, rot.y, 0);
                }
            } else {
                // forced 45 degree if there is no mouse to rotate (for mobile)
                transform.rotation = Quaternion.Euler(new Vector3(45, 0, 0));
            }

            // zoom
            float speed = Input.mousePresent ? zoomSpeedMouse : zoomSpeedTouch;
            float step = Utils.GetZoomUniversal() * speed;
            distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
        }

        // target follow
        transform.position = targetPos - (transform.rotation * Vector3.forward * distance);

        // avoid view blocking (disabled, see comment at the top)
        //RaycastHit hit;
        //if (Physics.Linecast(targetPos, transform.position, out hit, layerMask)) {
        //    // calculate a better distance (with some space between it)
        //    float d = Vector3.Distance(targetPos, hit.point) - 0.1f;
        //
        //    // set the final cam position
        //    transform.position = targetPos - (transform.rotation * Vector3.forward * d);
        //}
    }
}
