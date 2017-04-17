// The Entity class is rather simple. It contains a few basic entity properties
// like health, mana and level that all inheriting classes like Players and
// Monsters can use.
// 
// Entities also have a _target_ Entity that can't be synchronized with a
// SyncVar. Instead we created a EntityTargetSync component that takes care of
// that for us.
// 
// Entities use a deterministic finite state machine to handle IDLE/MOVING/DEAD/
// CASTING etc. states and events. Using a deterministic FSM means that we react
// to every single event that can happen in every state (as opposed to just
// taking care of the ones that we care about right now). This means a bit more
// code, but it also means that we avoid all kinds of weird situations like 'the
// monster doesn't react to a dead target when casting' etc.
// The next state is always set with the return value of the UpdateServer
// function. It can never be set outside of it, to make sure that all events are
// truly handled in the state machine and not outside of it. Otherwise we may be
// tempted to set a state in CmdBeingTrading etc., but would likely forget of
// special things to do depending on the current state.
//
// Entities also need a kinematic Rigidbody so that OnTrigger functions can be
// called. Note that there is currently a Unity bug that slows down the agent
// when having lots of FPS(300+) if the Rigidbody's Interpolate option is
// enabled. So for now it's important to disable Interpolation - which is a good
// idea in general to increase performance.
using UnityEngine;
#if UNITY_5_5_OR_NEWER // for people that didn't upgrade to 5.5. yet
using UnityEngine.AI;
#endif
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Serialization;
using System.Collections.Generic;

// note: no animator required, towers, dummies etc. may not have one
[RequireComponent(typeof(Rigidbody))] // kinematic, only needed for OnTrigger
[RequireComponent(typeof(NetworkProximityCheckerCustom))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkNavMeshAgent))]
public abstract class Entity : NetworkBehaviour {
    // finite state machine
    // -> state only writable by entity class to avoid all kinds of confusion
    [Header("State")]
    [SyncVar, SerializeField] string _state = "IDLE";
    public string state { get { return _state; } }

    // [SyncVar] NetworkIdentity: errors when null
    // [SyncVar] Entity: SyncVar only works for simple types
    // [SyncVar] GameObject is the only solution where we don't need a custom
    //           synchronization script (needs NetworkIdentity component!)
    // -> we still wrap it with a property for easier access, so we don't have
    //    to use target.GetComponent<Entity>() everywhere
    [Header("Target")]
    [SyncVar] GameObject _target;
    public Entity target {
        get { return _target != null  ? _target.GetComponent<Entity>() : null; }
        set { _target = value != null ? value.gameObject : null; }
    }

    [Header("Level")]
    [SyncVar] public int level = 1;
    
    [Header("Health")]
    [SyncVar, SerializeField] protected bool invincible = false; // GMs, Npcs, ...
    [SyncVar, SerializeField] protected bool healthRecovery = true; // can be disabled in combat etc.
    [SyncVar, SerializeField] protected int healthRecoveryRate = 1;
    [SyncVar                ] int _health = 1;
    public int health {
        get { return Mathf.Min(_health, healthMax); } // min in case hp>hpmax after buff ends etc.
        set { _health = Mathf.Clamp(value, 0, healthMax); }
    }
    public abstract int healthMax{ get; }

    [Header("Mana")]
    [SyncVar, SerializeField] protected bool manaRecovery = true; // can be disabled in combat etc.
    [SyncVar, SerializeField] protected int manaRecoveryRate = 1;
    [SyncVar                ] int _mana = 1;
    public int mana {
        get { return Mathf.Min(_mana, manaMax); } // min in case hp>hpmax after buff ends etc.
        set { _mana = Mathf.Clamp(value, 0, manaMax); }
    }
    public abstract int manaMax{ get; }

    [Header("Damage Popup")]
    [SerializeField] GameObject damagePopupPrefab;

    // other properties
    public float speed { get { return agent.speed; } }
    public abstract int damage { get; }
    public abstract int defense { get; }
    public abstract float blockChance { get; }
    public abstract float criticalChance { get; }

    // skill system for all entities (players, monsters, npcs, towers, ...)
    // 'skillTemplates' are the available skills (first one is default attack)
    // 'skills' are the loaded skills with cooldowns etc.
    [Header("Skills, Buffs, Status Effects")]
    public SkillTemplate[] skillTemplates;
    public SyncListSkill skills = new SyncListSkill();
    // current skill (synced because we need it as an animation parameter)
    [SyncVar] protected int currentSkill = -1;

    // cache
    [HideInInspector] public NavMeshAgent agent;
    [HideInInspector] public NetworkProximityChecker proxchecker;
    [HideInInspector] public NetworkIdentity netIdentity;
    [HideInInspector] public Animator animator;
    [HideInInspector] new public Collider collider;

    // networkbehaviour ////////////////////////////////////////////////////////
    // cache components on server and clients
    protected virtual void Awake() {
        agent = GetComponent<NavMeshAgent>();
        proxchecker = GetComponent<NetworkProximityChecker>();
        netIdentity = GetComponent<NetworkIdentity>();
        animator = GetComponent<Animator>();
        // the collider can also be a child in case of animated entities (where
        // it sits on the pelvis for example). equipment colliders etc. aren't
        // a problem because they are added after awake in case
        collider = GetComponentInChildren<Collider>();
    }

    public override void OnStartServer() {
        // health recovery every second
        InvokeRepeating("Recover", 1, 1);

        // HpDecreaseBy changes to "DEAD" state when hp drops to 0, but there is
        // a case where someone might instantiated a Entity with hp set to 0,
        // hence we have to check that case once at start
        if (health == 0) _state = "DEAD";
    }

    // entity logic will be implemented with a finite state machine
    // -> we should react to every state and to every event for correctness
    // -> we keep it functional for simplicity
    // note: can still use LateUpdate for Updates that should happen in any case
    void Update() {
        // monsters, npcs etc. don't have to be updated if no player is around
        // checking observers is enough, because lonely players have at least
        // themselves as observers, so players will always be updated
        // and dead monsters will respawn immediately in the first update call
        // even if we didn't update them in a long time (because of the 'end'
        // times)
        // -> update only if:
        //    - observers are null (they are null in clients)
        //    - if they are not null, then only if at least one (on server)
        //    - the entity is hidden, otherwise it would never be updated again
        //      because it would never get new observers
        // -> we also clear the target if it's hidden, so that players don't
        //    keep hidden (respawning) monsters as target, hence don't show them
        //    as target again when they are shown again
        if (netIdentity.observers == null || netIdentity.observers.Count > 0 || IsHidden()) {
            if (isClient) UpdateClient();
            if (isServer) {
                if (target != null && target.IsHidden()) target = null;
                _state = UpdateServer();
            }
        }
    }

    // update for server. should return the new state.
    protected abstract string UpdateServer();

    // update for client.
    protected abstract void UpdateClient();

    // visibility //////////////////////////////////////////////////////////////
    // hide a entity
    // note: using SetActive won't work because its not synced and it would
    //       cause inactive objects to not receive any info anymore
    // note: this won't be visible on the server as it always sees everything.
    [Server]
    public void Hide() {
        proxchecker.forceHidden = true;
    }

    [Server]
    public void Show() {
        proxchecker.forceHidden = false;
    }

    // is the entity currently hidden?
    // note: usually the server is the only one who uses forceHidden, the
    //       client usually doesn't know about it and simply doesn't see the
    //       GameObject.
    public bool IsHidden() {
        return proxchecker.forceHidden;
    }

    public float VisRange() {
        return proxchecker.visRange;
    }

    // look at a transform while only rotating on the Y axis (to avoid weird
    // tilts)
    public void LookAtY(Vector3 position) {
        transform.LookAt(new Vector3(position.x, transform.position.y, position.z));
    }
    
    // note: client can find out if moving by simply checking the state!
    [Server] // server is the only one who has up-to-date NavMeshAgent
    public bool IsMoving() {
        // -> agent.hasPath will be true if stopping distance > 0, so we can't
        //    really rely on that.
        // -> pathPending is true while calculating the path, which is good
        // -> remainingDistance is the distance to the last path point, so it
        //    also works when clicking somewhere onto a obstacle that isn'
        //    directly reachable.
        return agent.pathPending ||
               agent.remainingDistance > agent.stoppingDistance ||
               agent.velocity != Vector3.zero;
    }

    // health & mana ///////////////////////////////////////////////////////////
    public float HealthPercent() {
        return (health != 0 && healthMax != 0) ? (float)health / (float)healthMax : 0;
    }

    [Server]
    public void Revive(float healthPercentage = 1) {
        health = Mathf.RoundToInt(healthMax * healthPercentage);
    }
    
    public float ManaPercent() {
        return (mana != 0 && manaMax != 0) ? (float)mana / (float)manaMax : 0;
    }

    // combat //////////////////////////////////////////////////////////////////
    // no need to instantiate damage popups on the server
    enum PopupType { Normal, Block, Crit };
    [ClientRpc(channel=Channels.DefaultUnreliable)] // unimportant => unreliable
    void RpcShowDamagePopup(PopupType popupType, int amount, Vector3 position) {
        // spawn the damage popup (if any) and set the text
        // (-1 = block)
        if (damagePopupPrefab) {
            var popup = (GameObject)Instantiate(damagePopupPrefab, position, Quaternion.identity);
            if (popupType == PopupType.Normal)
                popup.GetComponentInChildren<TextMesh>().text = amount.ToString();
            else if (popupType == PopupType.Block)
                popup.GetComponentInChildren<TextMesh>().text = "<i>Block!</i>";
            else if (popupType == PopupType.Crit)
                popup.GetComponentInChildren<TextMesh>().text = amount + " Crit!";
        }
    }

    // deal damage at another entity
    // (can be overwritten for players etc. that need custom functionality)
    // (can also return the set of entities that were hit, just in case they are
    //  needed when overwriting it)
    [Server]
    public virtual HashSet<Entity> DealDamageAt(Entity entity, int amount, float aoeRadius=0) {
        // build the set of entities that were hit within AoE range
        var entities = new HashSet<Entity>();

        // add main target in any case, because non-AoE skills have radius=0
        entities.Add(entity);

        // add all targets in AoE radius around main target
        var colliders = Physics.OverlapSphere(entity.transform.position, aoeRadius); //, layerMask);
        foreach (var co in colliders) {
            var candidate = co.GetComponentInParent<Entity>();
            // overlapsphere cast uses the collider's bounding volume (see
            // Unity scripting reference), hence is often not exact enough
            // in our case (especially for radius 0.0). let's also check the
            // distance to be sure.
            if (candidate != null && candidate != this && candidate.health > 0 &&
                Vector3.Distance(entity.transform.position, candidate.transform.position) < aoeRadius)
                entities.Add(candidate);
        }

        // now deal damage at each of them
        foreach (var e in entities) {
            int damageDealt = 0;
            var popupType = PopupType.Normal;

            // don't deal any damage if target is invincible
            if (!e.invincible) {
                // block? (we use < not <= so that block rate 0 never blocks)
                if (Random.value < e.blockChance) {
                    popupType = PopupType.Block;
                // deal damage
                } else {
                    // subtract defense (but leave at least 1 damage, otherwise
                    // it may be frustrating for weaker players)
                    damageDealt = Mathf.Max(amount - e.defense, 1);

                    // critical hit?
                    if (Random.value < criticalChance) {
                        damageDealt *= 2;
                        popupType = PopupType.Crit;
                    }

                    // deal the damage
                    e.health -= damageDealt;
                }
            }

            // show damage popup in observers via ClientRpc
            // showing them above their head looks best, and we don't have to
            // use a custom shader to draw world space UI in front of the entity
            // note: we send the RPC to ourselves because whatever we killed
            //       might disappear before the rpc reaches it
            var bounds = e.GetComponentInChildren<Collider>().bounds;
            RpcShowDamagePopup(popupType, damageDealt, new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));

            // let's make sure to pull aggro in any case so that archers
            // are still attacked if they are outside of the aggro range
            e.OnAggro(this);
        }

        return entities;
    }

    // recovery ////////////////////////////////////////////////////////////////
    // receover health and mana once a second
    // (can be overwritten for players etc. that need custom functionality)
    // note: when stopping the server with the networkmanager gui, it will
    //       generate warnings that Recover was called on client because some
    //       entites will only be disabled but not destroyed. let's not worry
    //       about that for now.
    [Server]
    public virtual void Recover() {
        if (enabled && health > 0) {
            if (healthRecovery) health += healthRecoveryRate;
            if (manaRecovery) mana += manaRecoveryRate;
        }
    }

    // aggro ///////////////////////////////////////////////////////////////////
    // this function is called by the AggroArea (if any) on clients and server
    public virtual void OnAggro(Entity entity) {}

    // skill system ////////////////////////////////////////////////////////////
    // fist fights are virtually pointless because they overcomplicate the code
    // and they don't add any value to the game. so we need a check to find out
    // if the entity currently has a weapon equipped, otherwise casting a skill
    // shouldn't be possible. this may always return true for monsters, towers
    // etc.
    public abstract bool HasCastWeapon();

    // we can't have a public array of types that we can modify in the Inspector
    // so we need an abstract function to check if players can attack players,
    // monsters, npcs etc.
    public abstract bool CanAttackType(System.Type type);

    // the first check validates the caster
    // (the skill won't be ready if we check self while casting it. so the
    //  checkSkillReady variable can be used to ignore that if needed)
    public bool CastCheckSelf(Skill skill, bool checkSkillReady = true) {
        // has a weapon (important for projectiles etc.), no cooldown, hp, mp?
        return HasCastWeapon() &&
               (!checkSkillReady || skill.IsReady()) &&
               health > 0 &&
               mana >= skill.manaCosts;
    }

    // the second check validates the target and corrects it for the skill if
    // necessary (e.g. when trying to heal an npc, it sets target to self first)
    public bool CastCheckTarget(Skill skill) {
        // attack: target exists, alive, not self, oktype
        // (we can't have a public array of types that we can modify
        //  in the Inspector, so we need an abstract function)                
        if (skill.category == "Attack") {
            return target != null &&
                   target != this &&
                   target.health > 0 &&
                   CanAttackType(target.GetType());
        // heal: on target? (if exists, not self, type) or self
        } else if (skill.category == "Heal") {
            if (target != null &&
                target != this &&
                target.GetType() == this.GetType()) {
                // can only heal the target if it's not dead 
                return target.health > 0;
            // otherwise we want to heal ourselves, which is always allowed
            // (we already checked if we are alive in castcheckself)
            } else {
                target = this;
                return true;
            }
        // buff: only buff self => ok
        } else if (skill.category == "Buff") {
            target = this;
            return true;
        }
        // otherwise the category is invalid
        Debug.LogWarning("invalid skill category for: " + skill.name);
        return false;
    }

    // the third check validates the distance between the caster and the target
    // (in case of buffs etc., the target was already corrected to 'self' by
    //  castchecktarget, hence we don't have to worry about anything here)
    public bool CastCheckDistance(Skill skill) {
        return target != null &&
               Utils.ClosestDistance(collider, target.collider) <= skill.castRange;
    }

    // applies the skill effects. casting and waiting has to be done in the
    // state machine
    public void CastSkill(Skill skill) {
        // check self again (alive, mana, weapon etc.). ignoring the skill cd
        // and check target again
        // note: we don't check the distance again. the skill will be cast even
        // if the target walked a bit while we casted it (it's simply better
        // gameplay and less frustrating)
        if (CastCheckSelf(skill, false) && CastCheckTarget(skill)) {
            // attack
            if (skill.category == "Attack") {
                // decrease mana in any case
                mana -= skill.manaCosts;

                // deal damage directly or shoot a projectile?
                if (skill.projectile == null) {
                    // deal damage directly
                    DealDamageAt(target, damage + skill.damage, skill.aoeRadius);
                } else {
                    // spawn the projectile and shoot it towards target
                    // (make sure that the weapon prefab has a ProjectileMount
                    //  somewhere in the hierarchy)
                    var mount = transform.FindRecursively("ProjectileMount");
                    if (mount != null) {
                        var position = mount.position;
                        var go = (GameObject)Instantiate(skill.projectile.gameObject, position, Quaternion.identity);
                        var projectile = go.GetComponent<Projectile>();
                        projectile.target = target;
                        projectile.caster = this;
                        projectile.damage = damage + skill.damage;
                        projectile.aoeRadius = skill.aoeRadius;
                        NetworkServer.Spawn(go);
                    } else {
                        Debug.LogWarning(name + " has no ProjectileMount. Can't fire the projectile.");
                    }
                }
            // heal
            } else if (skill.category == "Heal") {
                // note: 'target alive' checks were done above already
                mana -= skill.manaCosts;
                target.health += skill.healsHealth;
                target.mana += skill.healsMana;
            // buff
            } else if (skill.category == "Buff") {
                // set the buff end time (the rest is done in .damage etc.)
                mana -= skill.manaCosts;
                skill.buffTimeEnd = Time.time + skill.buffTime;
            }

            // start the cooldown (and save it in the struct)
            skill.cooldownEnd = Time.time + skill.cooldown;

            // save any skill modifications in any case
            skills[currentSkill] = skill;
        } else {
            // not all requirements met. no need to cast the same skill again
            currentSkill = -1;
        }
    }

    // helper function to stop all buffs if needed (e.g. in OnDeath)
    public void StopBuffs() {
        for (int i = 0; i < skills.Count; ++i) {
            if (skills[i].category == "Buff") { // not for Murder status etc.
                var skill = skills[i];
                skill.buffTimeEnd = Time.time;
                skills[i] = skill;
            }
        }
    }
}
