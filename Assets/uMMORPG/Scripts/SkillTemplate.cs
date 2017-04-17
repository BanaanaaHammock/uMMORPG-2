// Saves the skill info in a ScriptableObject that can be used ingame by
// referencing it from a MonoBehaviour. It only stores an skill's static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some skills may not be referenced by any
// entity ingame (e.g. after a special event). But all skills should still be
// loadable from the database, even if they are not referenced by anyone
// anymore. So we have to use Resources.Load. (before we added them to the dict
// in OnEnable, but that's only called for those that are referenced in the
// game. All others will be ignored be Unity.)
//
// Skills can have different stats for each skill level. This is what the
// 'levels' list is for. If you only need one level, then only add one entry to
// it in the Inspector.
//
// A Skill can be created by right clicking the Resources folder and selecting
// Create -> uMMORPG Skill. Existing skills can be found in the Resources folder
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName="New Skill", menuName="uMMORPG Skill", order=999)]
public class SkillTemplate : ScriptableObject {
    [Header("Base Stats")]
    // we can use the category to decide what to do on use. example categories:
    // Attack, Stun, Buff, Heal, ...
    public string category;
    public bool followupDefaultAttack;
    [TextArea(1, 30)] public string toolTip;
    public Sprite image;
    public bool learnDefault; // normal attack etc.

    [System.Serializable]
    public struct SkillLevel {
        // level dependent stats
        public int damage;
        public float castTime;
        public float cooldown;
        public float castRange;
        public float aoeRadius;
        public int manaCosts;
        public int healsHealth;
        public int healsMana;
        public float buffTime;
        public int buffsHealthMax;
        public int buffsManaMax;
        public int buffsDamage;
        public int buffsDefense;
        [Range(0, 1)] public float buffsBlockChance;
        [Range(0, 1)] public float buffsCriticalChance;
        public float buffsHealthPercentPerSecond; // 0.1=10%; can be negative too
        public float buffsManaPercentPerSecond; // 0.1=10%; can be negative too
        public Projectile projectile; // Arrows, Bullets, Fireballs, ...

        // learning requirements
        public int requiredLevel;
        public long requiredSkillExperience;
    }
    [Header("Skill Levels")]
    public SkillLevel[] levels = new SkillLevel[]{new SkillLevel()}; // default

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    static Dictionary<string, SkillTemplate> cache = null;
    public static Dictionary<string, SkillTemplate> dict {
        get {
            // load if not loaded yet
            return cache ?? (cache = Resources.LoadAll<SkillTemplate>("").ToDictionary(
                skill => skill.name, skill => skill)
            );
        }
    }
}
