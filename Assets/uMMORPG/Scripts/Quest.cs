// The Quest struct only contains the dynamic quest properties and a name, so
// that the static properties can be read from the scriptable object. The
// benefits are low bandwidth and easy Player database saving (saves always
// refer to the scriptable quest, so we can change that any time).
//
// Quests have to be structs in order to work with SyncLists.
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public struct Quest {
    // name used to reference the database entry (cant save template directly
    // because synclist only support simple types)
    public string name;

    // dynamic stats
    public int killed;
    public bool completed; // after finishing it at the npc and getting rewards

    // constructors
    public Quest(QuestTemplate template) {
        name = template.name;
        killed = 0;
        completed = false;
    }

    // does the template still exist?
    public bool TemplateExists() {
        return QuestTemplate.dict.ContainsKey(name);
    }

    // database quest property access
    public QuestTemplate template {
        get { return QuestTemplate.dict[name]; }
    }
    public int requiredLevel {
        get { return template.requiredLevel; }
    }
    public string predecessor {
        get { return template.predecessor != null ? template.predecessor.name : ""; }
    }
    public long rewardGold {
        get { return template.rewardGold; }
    }
    public long rewardExperience {
        get { return template.rewardExperience; }
    }
    public ItemTemplate rewardItem {
        get { return template.rewardItem; }
    }
    public string killName {
        get { return template.killTarget != null ? template.killTarget.name : ""; }
    }
    public int killAmount {
        get { return template.killAmount; }
    }
    public string gatherName {
        get { return template.gatherItem != null ? template.gatherItem.name : ""; }
    }
    public int gatherAmount {
        get { return template.gatherAmount; }
    }

    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code. we can't do it in
    // QuestTemplate because some variables can only be replaced here, hence we
    // would end up with some variables not replaced in the string when calling
    // Tooltip() from the template.
    // -> note: each tooltip can have any variables, or none if needed
    // -> example usage:
    /*
    <b>{NAME}</b>
    Description here...

    Tasks:
    * Kill {KILLNAME}: {KILLED}/{KILLAMOUNT}
    * Gather {GATHERNAME}: {GATHERED}/{GATHERAMOUNT}

    Rewards:
    * {REWARDGOLD} Gold
    * {REWARDEXPERIENCE} Experience
    * {REWARDITEM}

    {STATUS}
    */
    public string ToolTip(int gathered = 0) {
        string tip = template.toolTip;
        tip = tip.Replace("{NAME}", name);
        tip = tip.Replace("{KILLNAME}", killName);
        tip = tip.Replace("{KILLAMOUNT}", killAmount.ToString());
        tip = tip.Replace("{GATHERNAME}", gatherName);
        tip = tip.Replace("{GATHERAMOUNT}", gatherAmount.ToString());
        tip = tip.Replace("{REWARDGOLD}", rewardGold.ToString());
        tip = tip.Replace("{REWARDEXPERIENCE}", rewardExperience.ToString());
        tip = tip.Replace("{REWARDITEM}", rewardItem != null ? rewardItem.name : "");
        tip = tip.Replace("{KILLED}", killed.ToString());
        tip = tip.Replace("{GATHERED}", gathered.ToString());
        tip = tip.Replace("{STATUS}", IsFulfilled(gathered) ? "<i>Completed!</i>" : "");
        return tip;
    }

    // a quest is fulfilled if all requirements were met and it can be completed
    // at the npc
    public bool IsFulfilled(int gathered) {
        return killed >= killAmount && gathered >= gatherAmount;
    }
}

public class SyncListQuest : SyncListStruct<Quest> { }
