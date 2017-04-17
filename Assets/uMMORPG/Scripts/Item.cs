// The Item struct only contains the dynamic item properties and a name, so that
// the static properties can be read from the scriptable object.
//
// Items have to be structs in order to work with SyncLists.
//
// The player inventory actually needs Item slots that can sometimes be empty
// and sometimes contain an Item. The obvious way to do this would be a
// InventorySlot class that can store an Item, but SyncLists only work with
// structs - so the Item struct needs an option to be _empty_ to act like a
// slot. The simple solution to it is the _valid_ property in the Item struct.
// If valid is false then this Item is to be considered empty.
//
// _Note: the alternative is to have a list of Slots that can contain Items and
// to serialize them manually in OnSerialize and OnDeserialize, but that would
// be a whole lot of work and the workaround with the valid property is much
// simpler._
//
// Items can be compared with their name property, two items are the same type
// if their names are equal.
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public struct Item {
    // name used to reference the database entry (cant save template directly
    // because synclist only support simple types)
    public string name;

    // dynamic stats (cooldowns etc. later)
    public bool valid; // acts as slot. false means there is no item in here.
    public int amount;

    // constructors
    public Item(ItemTemplate template, int amount=1) {
        name = template.name;
        this.amount = amount;
        valid = true;
    }

    // does the template still exist?
    public bool TemplateExists() {
        return name != null && ItemTemplate.dict.ContainsKey(name);
    }

    // database item property access
    public ItemTemplate template {
        get { return ItemTemplate.dict[name]; }
    }
    public string category {
        get { return template.category; }
    }
    public int maxStack {
        get { return template.maxStack; }
    }
    public long buyPrice {
        get { return template.buyPrice; }
    }
    public long sellPrice {
        get { return template.sellPrice; }
    }
    public int minLevel {
        get { return template.minLevel; }
    }
    public bool sellable {
        get { return template.sellable; }
    }
    public bool tradable {
        get { return template.tradable; }
    }
    public bool destroyable {
        get { return template.destroyable; }
    }
    public Sprite image {
        get { return template.image; }
    }
    public bool usageDestroy {
        get { return template.usageDestroy; }
    }
    public int usageHealth {
        get { return template.usageHealth; }
    }
    public int usageMana {
        get { return template.usageMana; }
    }
    public int usageExperience {
        get { return template.usageExperience; }
    }
    public int equipHealthBonus {
        get { return template.equipHealthBonus; }
    }
    public int equipManaBonus {
        get { return template.equipManaBonus; }
    }
    public int equipDamageBonus {
        get { return template.equipDamageBonus; }
    }
    public int equipDefenseBonus {
        get { return template.equipDefenseBonus; }
    }
    public float equipBlockChanceBonus {
        get { return template.equipBlockChanceBonus; }
    }
    public float equipCriticalChanceBonus {
        get { return template.equipCriticalChanceBonus; }
    }
    public GameObject modelPrefab {
        get { return template.modelPrefab; }
    }

    // fill in all variables into the tooltip
    // this saves us lots of ugly string concatenation code. we can't do it in
    // ItemTemplate because some variables can only be replaced here, hence we
    // would end up with some variables not replaced in the string when calling
    // Tooltip() from the template.
    // -> note: each tooltip can have any variables, or none if needed
    // -> example usage:
    /*
    <b>{NAME}</b>
    Description here...

    {EQUIPDAMAGEBONUS} Damage
    {EQUIPDEFENSEBONUS} Defense
    {EQUIPHEALTHBONUS} Health
    {EQUIPMANABONUS} Mana
    {EQUIPBLOCKCHANCEBONUS} Block
    {EQUIPCRITICALCHANCEBONUS} Critical
    Restores {USAGEHEALTH} Health on use.
    Restores {USAGEMANA} Mana on use.
    Grants {USAGEEXPERIENCE} Experience on use.
    Destroyable: {DESTROYABLE}
    Sellable: {SELLABLE}
    Tradable: {TRADABLE}
    Required Level: {MINLEVEL}

    Amount: {AMOUNT}
    Price: {BUYPRICE} Gold
    <i>Sells for: {SELLPRICE} Gold</i>
    */
    public string ToolTip() {
        string tip = template.toolTip;
        tip = tip.Replace("{NAME}", name);
        tip = tip.Replace("{CATEGORY}", category);
        tip = tip.Replace("{EQUIPDAMAGEBONUS}", equipDamageBonus.ToString());
        tip = tip.Replace("{EQUIPDEFENSEBONUS}", equipDefenseBonus.ToString());
        tip = tip.Replace("{EQUIPHEALTHBONUS}", equipHealthBonus.ToString());
        tip = tip.Replace("{EQUIPMANABONUS}", equipManaBonus.ToString());
        tip = tip.Replace("{EQUIPBLOCKCHANCEBONUS}", Mathf.RoundToInt(equipBlockChanceBonus * 100).ToString());
        tip = tip.Replace("{EQUIPCRITICALCHANCEBONUS}", Mathf.RoundToInt(equipCriticalChanceBonus * 100).ToString());
        tip = tip.Replace("{USAGEHEALTH}", usageHealth.ToString());
        tip = tip.Replace("{USAGEMANA}", usageMana.ToString());
        tip = tip.Replace("{USAGEEXPERIENCE}", usageExperience.ToString());
        tip = tip.Replace("{DESTROYABLE}", (destroyable ? "Yes" : "No"));
        tip = tip.Replace("{SELLABLE}", (sellable ? "Yes" : "No"));
        tip = tip.Replace("{TRADABLE}", (tradable ? "Yes" : "No"));
        tip = tip.Replace("{MINLEVEL}", minLevel.ToString());
        tip = tip.Replace("{BUYPRICE}", buyPrice.ToString());
        tip = tip.Replace("{SELLPRICE}", sellPrice.ToString());
        tip = tip.Replace("{AMOUNT}", amount.ToString());
        return tip;
    }
}

public class SyncListItem : SyncListStruct<Item> { }
