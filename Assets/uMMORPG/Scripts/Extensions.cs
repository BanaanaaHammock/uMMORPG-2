// This class adds functions to built-in types.
using UnityEngine;
using UnityEngine.UI;
#if UNITY_5_5_OR_NEWER // for people that didn't upgrade to 5.5. yet
using UnityEngine.AI;
#endif
using UnityEngine.Events;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using System.Linq;

public static class Extensions {
    
    // string to int (returns errVal if failed)
    public static int ToInt(this string value, int errVal=0) {
        Int32.TryParse(value, out errVal);
        return errVal;
    }
    
    // string to long (returns errVal if failed)
    public static long ToLong(this string value, long errVal=0) {
        Int64.TryParse(value, out errVal);
        return errVal;
    }

    // write xml node: <localname>value</localname>
    public static void WriteElementValue(this XmlWriter writer, string localName, object value) {
        writer.WriteStartElement(localName);
        writer.WriteValue(value);
        writer.WriteEndElement();
    }

    // write xml node: <localname>Serialize(object)</localname>
    public static void WriteElementObject(this XmlWriter writer, string localName, object value) {
        writer.WriteStartElement(localName);
        var serializer = new XmlSerializer(value.GetType());
        serializer.Serialize(writer, value);
        writer.WriteEndElement();
    }

    // read xml node: <...>Unserialize(object)</...>
    public static T ReadElementObject<T>(this XmlReader reader) {
        var serializer = new XmlSerializer(typeof(T));
        reader.ReadStartElement();
        T value = (T)serializer.Deserialize(reader);
        reader.ReadEndElement();
        return value;
    }

    // transform.Find only finds direct children, no grandchildren etc.
    public static Transform FindRecursively(this Transform transform, string name) {
        return Array.Find(transform.GetComponentsInChildren<Transform>(true),
                          t => t.name == name);
    }

    // FindIndex function for synclists
    public static int FindIndex<T>(this SyncList<T> list, Predicate<T> match) {
        for (int i = 0; i < list.Count; ++i)
            if (match(list[i]))
                return i;
        return -1;
    }

    // UI SetListener extension that removes previous and then adds new listener
    // (this version is for onClick etc.)
    public static void SetListener(this UnityEvent uEvent, UnityAction call) {
        uEvent.RemoveAllListeners();
        uEvent.AddListener(call);
    }

    // UI SetListener extension that removes previous and then adds new listener
    // (this version is for onEndEdit, onValueChanged etc.)
    public static void SetListener<T>(this UnityEvent<T> uEvent, UnityAction<T> call) {
        uEvent.RemoveAllListeners();
        uEvent.AddListener(call);
    }

    // NetworkIdentity find observer with name
    public static GameObject FindObserver(this NetworkIdentity ni, string observerName) {
        var pc = ni.observers.FirstOrDefault(c => c.playerControllers.Count > 0 &&
                                                  c.playerControllers[0].gameObject.name == observerName);
        return pc != null ? pc.playerControllers[0].gameObject : null;
    }

    // NavMeshAgent helper function that returns the nearest valid point for a
    // given destination. This is really useful for click & wsad movement
    // because the player may click into all kinds of unwalkable paths:
    //
    //       ________
    //      |xxxxxxx|
    //      |x|   |x|
    // P   A|B| C |x|
    //      |x|___|x|
    //      |xxxxxxx|
    //
    // if a player is at position P and tries to go to:
    // - A: the path is walkable, everything is fine
    // - C: C is on a NavMesh, but we can't get there directly. CalcualatePath
    //      will return A as the last walkable point
    // - B: B is not on a NavMesh, CalulatePath doesn't work here. We need to
    //   find the nearest point on a NavMesh first (might be A or C) and then
    //   calculate the nearest valid one (A)
    public static Vector3 NearestValidDestination(this NavMeshAgent agent, Vector3 destination) {
        // can we calculate a path there? then return the closest valid point
        var path = new NavMeshPath();
        if (agent.CalculatePath(destination, path))
            return path.corners[path.corners.Length - 1];

        // otherwise find nearest navmesh position first. we use a radius of
        // speed*2 which works fine. afterwards we find the closest valid point.
        NavMeshHit hit;
        if (NavMesh.SamplePosition(destination, out hit, agent.speed * 2, NavMesh.AllAreas))
            if (agent.CalculatePath(hit.position, path))
                return path.corners[path.corners.Length - 1];

        // nothing worked, don't go anywhere.
        return agent.transform.position;
    }

    // check if a list has duplicates
    // new List<int>(){1, 2, 2, 3}.HasDuplicates() => true
    // new List<int>(){1, 2, 3, 4}.HasDuplicates() => false
    // new List<int>().HasDuplicates() => false
    public static bool HasDuplicates<T>(this List<T> list) {
        return list.Count != list.Distinct().Count();
    }
}
