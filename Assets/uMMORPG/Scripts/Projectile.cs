// This class is for bullets, arrows, fireballs and so on.
using UnityEngine;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour {
    [SerializeField] float speed = 1;

    // only the server needs the target, no need for a SyncVar
    // (can't sync Entity anyway)
    [HideInInspector] public Entity target;   // set by script
    [HideInInspector] public Entity caster;   // set by script
    [HideInInspector] public int damage;      // set by script
    [HideInInspector] public float aoeRadius; // set by script

    // update here already so that it doesn't spawn with a weird rotation
    [ServerCallback]
    void Start() { FixedUpdate(); }

    [ServerCallback]
    void FixedUpdate() {
        // target and caster still around?
        // note: we keep flying towards it even if it died already, because
        //       it looks weird if fireballs would be canceled inbetween.
        if (target != null && caster != null) {
            // move closer and look at the target
            var goal = target.GetComponentInChildren<Collider>().bounds.center;
            transform.position = Vector3.MoveTowards(transform.position, goal, speed);
            transform.LookAt(goal);

            // reached it?
            if (transform.position == goal) {
                // still alive? deal damage
                if (target.health > 0) caster.DealDamageAt(target, damage, aoeRadius);
                // done, destroy self
                NetworkServer.Destroy(gameObject);
            }
        } else {
            // destroy self
            NetworkServer.Destroy(gameObject);
        }
    }
}
