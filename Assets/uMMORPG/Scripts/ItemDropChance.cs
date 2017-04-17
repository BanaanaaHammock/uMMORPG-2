// Defines the drop chance of an item for monster loot generation.
using UnityEngine;

[System.Serializable]
public class ItemDropChance {
    public ItemTemplate template;
    [Range(0,1)] public float probability;
}
