// Contains all the network messages that we need.
using UnityEngine;
using UnityEngine.Networking;

// client to server ////////////////////////////////////////////////////////////
public class LoginMsg : MessageBase {
    public static short MsgId = 1000;
    public string id;
    public string pw;
}

public class CharacterSelectMsg : MessageBase {
    public static short MsgId = 1001;
    public int index;
}

public class CharacterDeleteMsg : MessageBase {
    public static short MsgId = 1002;
    public int index;
}

public class CharacterCreateMsg : MessageBase {
    public static short MsgId = 1003;
    public string name;
    public int classIndex;
}

// server to client ////////////////////////////////////////////////////////////
// we need an error msg packet because we can't use TargetRpc with the Network-
// Manager, since it's not a MonoBehaviour.
public class ErrorMsg : MessageBase {
    public static short MsgId = 2000;
    public string text;
    public bool causesDisconnect;
}

public class CharactersAvailableMsg : MessageBase {
    public static short MsgId = 2001;
    public string[] characterNames;   // one name per character
    public string[] characterClasses; // one class per character (=the prefab name)
}