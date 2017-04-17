// Saves the quest info in a ScriptableObject that can be used ingame by
// referencing it from a MonoBehaviour. It only stores an quest's static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some quests may not be referenced by any
// entity ingame (e.g. after a special event). But all quests should still be
// loadable from the database, even if they are not referenced by anyone
// anymore. So we have to use Resources.Load. (before we added them to the dict
// in OnEnable, but that's only called for those that are referenced in the
// game. All others will be ignored be Unity.)
//
// A Quest can be created by right clicking the Resources folder and selecting
// Create -> uMMORPG Quest. Existing quests can be found in the Resources folder
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName="New Quest", menuName="uMMORPG Quest", order=999)]
public class QuestTemplate : ScriptableObject {
    [Header("General")]
    [TextArea(1, 30)] public string toolTip;

    [Header("Requirements")]
    public int requiredLevel; // player.level
    public QuestTemplate predecessor; // this quest has to be completed first

    [Header("Rewards")]
    public long rewardGold;
    public long rewardExperience;
    public ItemTemplate rewardItem;

    [Header("Fulfillment")]
    public Monster killTarget;
    public int killAmount;
    public ItemTemplate gatherItem;
    public int gatherAmount;

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    static Dictionary<string, QuestTemplate> cache = null;
    public static Dictionary<string, QuestTemplate> dict {
        get {
            // load if not loaded yet
            return cache ?? (cache = Resources.LoadAll<QuestTemplate>("").ToDictionary(
                quest => quest.name, quest => quest)
            );
        }
    }
}
