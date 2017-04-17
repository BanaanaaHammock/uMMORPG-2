// Draws the agent's path as Gizmo.
using UnityEngine;
#if UNITY_5_5_OR_NEWER // for people that didn't upgrade to 5.5. yet
using UnityEngine.AI;
#endif

[RequireComponent(typeof(NavMeshAgent))]
public class NavmeshPathGizmo : MonoBehaviour {
    
    void OnDrawGizmos() {
        var agent = GetComponent<NavMeshAgent>();
        var path = agent.path;

        // color depends on status
        Color c = Color.white;
        switch (path.status) {
            case NavMeshPathStatus.PathComplete: c = Color.white; break;
            case NavMeshPathStatus.PathInvalid: c = Color.red; break;
            case NavMeshPathStatus.PathPartial: c = Color.yellow; break;
        }

        // draw the path
        for (int i = 1; i < path.corners.Length; ++i)
            Debug.DrawLine(path.corners[i-1], path.corners[i], c);
    }
}