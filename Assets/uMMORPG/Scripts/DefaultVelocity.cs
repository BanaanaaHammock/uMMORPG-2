// Sets the Rigidbody's velocity in Start().
using UnityEngine;

public class DefaultVelocity : MonoBehaviour {
    public Vector3 velocity;
    
    void Start() {
        GetComponent<Rigidbody>().velocity = velocity;
    }
}
