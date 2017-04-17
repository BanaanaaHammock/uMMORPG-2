// Saves the item info in a ScriptableObject that can be used ingame by
// referencing it from a MonoBehaviour. It only stores an item's static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some items may not be referenced by any
// entity ingame (e.g. when a special event item isn't dropped anymore after the
// event). But all items should still be loadable from the database, even if
// they are not referenced by anyone anymore. So we have to use Resources.Load.
// (before we added them to the dict in OnEnable, but that's only called for
//  those that are referenced in the game. All others will be ignored be Unity.)
//
// An Item can be created by right clicking the Resources folder and selecting
// Create -> uMMORPG Item. Existing items can be found in the Resources folder.
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName="New Item", menuName="uMMORPG Item", order=999)]
public class ItemTemplate : ScriptableObject {
    [Header("Base Stats")]
    public string category;
    public int maxStack;
    public long buyPrice;
    public long sellPrice;
    public long itemMallPrice; // set >0 to appear in item mall
    public int minLevel; // level required to use/equip the item
    public bool sellable;
    public bool tradable;
    public bool destroyable;
    [TextArea(1, 30)] public string toolTip;
    public Sprite image;

    [Header("Usage Boosts")]
    public bool usageDestroy;
    public int usageHealth;
    public int usageMana;
    public int usageExperience;

    [Header("Equipment Boosts")]
    public int equipHealthBonus;
    public int equipManaBonus;
    public int equipDamageBonus;
    public int equipDefenseBonus;
    [Range(0, 1)] public float equipBlockChanceBonus;
    [Range(0, 1)] public float equipCriticalChanceBonus;
    public GameObject modelPrefab;

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    static Dictionary<string, ItemTemplate> cache = null;
    public static Dictionary<string, ItemTemplate> dict {
        get {
            // load if not loaded yet
            return cache ?? (cache = Resources.LoadAll<ItemTemplate>("").ToDictionary(
                item => item.name, item => item)
            );
        }
    }
    
    // inspector validation ////////////////////////////////////////////////////
    void OnValidate() {
        // make sure that the sell price <= buy price to avoid exploitation
        // (people should never buy an item for 1 gold and sell it for 2 gold)
        sellPrice = Math.Min(sellPrice, buyPrice);
    }
}
