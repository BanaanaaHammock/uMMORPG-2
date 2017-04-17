// Synchronizing an entity's name is crucial for components that need the proper
// name in the Start function (e.g. to load the skillbar by name).
//
// Simply using OnSerialize and OnDeserialize is the easiest way to do it. Using
// a SyncVar would require Start, Hooks etc.
using UnityEngine;
using UnityEngine.Networking;

[NetworkSettings(channel=Channels.DefaultUnreliable)] // unreliable is enough
public class NetworkName : NetworkBehaviour {
    // server-side serialization
    //
    // I M P O R T A N T
    //
    // always read and write the same amount of bytes. never let any errors
    // happen. otherwise readstr/readbytes out of range bugs happen.
    public override bool OnSerialize(NetworkWriter writer, bool initialState) {
        writer.Write(name);
        return true;
    }

    // client-side deserialization
    //
    // I M P O R T A N T
    //
    // always read and write the same amount of bytes. never let any errors
    // happen. otherwise readstr/readbytes out of range bugs happen.
    public override void OnDeserialize(NetworkReader reader, bool initialState) {
        name = reader.ReadString();
    }
}
