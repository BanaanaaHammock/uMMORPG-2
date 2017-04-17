// UNET's current NetworkTransform is really laggy, so we make it smooth by
// simply synchronizing the agent's destination. We could also lerp between
// the transform positions, but this is much easier and saves lots of bandwidth.
//
// Using a NavMeshAgent also has the benefit that no rotation has to be synced
// while moving.
//
// Notes:
//
// - Teleportations have to be detected and synchronized properly
// - Caching the agent won't work because serialization sometimes happens
// before awake/start
// - We also need the stopping distance, otherwise entities move too far.
using UnityEngine;
#if UNITY_5_5_OR_NEWER // for people that didn't upgrade to 5.5. yet
using UnityEngine.AI;
#endif
using UnityEngine.Networking;

[RequireComponent(typeof(NavMeshAgent))]
// unreliable is enough and we want
// to send changes immediately. everything else causes lags.
[NetworkSettings(channel=Channels.DefaultUnreliable, sendInterval=0)]
public class NetworkNavMeshAgent : NetworkBehaviour {
    Vector3 lastDestination; // for dirty bit
    bool hadPath = false; // had path since last time? for warp detection

    // find out if destination changed on server
    [ServerCallback]
    void Update() {
        var agent = GetComponent<NavMeshAgent>();
        if (agent.hasPath || agent.pathPending) hadPath = true;
        if (agent.destination != lastDestination)
            SetDirtyBit(1);
    }

    // server-side serialization
    //
    // I M P O R T A N T
    //
    // always read and write the same amount of bytes. never let any errors
    // happen. otherwise readstr/readbytes out of range bugs happen.
    public override bool OnSerialize(NetworkWriter writer, bool initialState) {
        var agent = GetComponent<NavMeshAgent>();
        writer.Write(transform.position); // for rubberbanding
        writer.Write(agent.destination);
        writer.Write(agent.speed);
        writer.Write(agent.stoppingDistance);
        writer.Write(agent.destination != lastDestination && !hadPath); // warped? avoid sliding to respawn point etc.

        // reset helpers
        lastDestination = agent.destination;
        hadPath = false;

        return true;
    }

    // client-side deserialization
    //
    // I M P O R T A N T
    //
    // always read and write the same amount of bytes. never let any errors
    // happen. otherwise readstr/readbytes out of range bugs happen.
    public override void OnDeserialize(NetworkReader reader, bool initialState) {
        var agent              = GetComponent<NavMeshAgent>();
        var pos                = reader.ReadVector3();
        var dest               = reader.ReadVector3();
        agent.speed            = reader.ReadSingle();
        agent.stoppingDistance = reader.ReadSingle();
        bool warped            = reader.ReadBoolean();

        // OnDeserialize must always return so that next one is called too
        try {
            // only try to set the destination if the agent is on a navmesh already
            // (it might not when falling from the sky after joining)
            if (agent.isOnNavMesh) {
                // warp if necessary. distance check to filter out false positives
                if (warped && Vector3.Distance(pos, transform.position) > agent.radius)
                    agent.Warp(pos); // to pos is always smoother

                // rubberbanding: if we are too far off because of a rapid position
                // change or latency, then warp
                // -> agent moves 'speed' meter per seconds
                // -> if we are 2 speed units behind, then we teleport
                //    (using speed is better than using a hardcoded value)
                if (Vector3.Distance(transform.position, pos) > agent.speed * 2)
                    agent.Warp(pos);

                // set destination afterwards, so that we never stop going there
                // even after being warped etc.
                agent.destination = dest;
            }
        } catch {}
    }
}
