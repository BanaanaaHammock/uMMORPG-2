// The default NetworkProximityChecker requires a collider on the same object,
// but in some cases we want to put the collider onto a child object (e.g. for
// animations).
//
// We modify the NetworkProximityChecker source from BitBucket to support
// colliders on child objects by searching the NetworkIdentity in parents.
//
// Note: requires at least Unity 5.3.5, otherwise there is IL2CPP bug #786499.
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;

public class NetworkProximityCheckerCustom : NetworkProximityChecker {
    // function from bitbucket
    public override bool OnRebuildObservers(HashSet<NetworkConnection> observers, bool initial)
    {
        if (forceHidden)
        {
            // ensure player can still see themself
            var uv = GetComponent<NetworkIdentity>();
            if (uv.connectionToClient != null)
            {
                observers.Add(uv.connectionToClient);
            }
            return true;
        }

        // find players within range
        switch (checkMethod)
        {
            case CheckMethod.Physics3D:
            {
                var hits = Physics.OverlapSphere(transform.position, visRange);
                foreach (var hit in hits)
                {
                    // (if an object has a connectionToClient, it is a player)
                    //var uv = hit.GetComponent<NetworkIdentity>();             <----- DEFAULT
                    var uv = hit.GetComponentInParent<NetworkIdentity>(); //    <----- MODIFIED
                    if (uv != null && uv.connectionToClient != null)
                    {
                        observers.Add(uv.connectionToClient);
                    }
                }
                return true;
            }

            case CheckMethod.Physics2D:
            {
                var hits = Physics2D.OverlapCircleAll(transform.position, visRange);
                foreach (var hit in hits)
                {
                    // (if an object has a connectionToClient, it is a player)
                    //var uv = hit.GetComponent<NetworkIdentity>();             <----- DEFAULT
                    var uv = hit.GetComponentInParent<NetworkIdentity>(); //    <----- MODIFIED
                    if (uv != null && uv.connectionToClient != null)
                    {
                        observers.Add(uv.connectionToClient);
                    }
                }
                return true;
            }
        }
        return false;
    }
}
