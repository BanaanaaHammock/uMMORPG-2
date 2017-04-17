// Saves the crafting recipe info in a ScriptableObject that can be used ingame
// by referencing it from a MonoBehaviour. It only stores static data.
//
// We also add each one to a dictionary automatically, so that all of them can
// be found by name without having to put them all in a database. Note that we
// have to put them all into the Resources folder and use Resources.LoadAll to
// load them. This is important because some recipes may not be referenced by
// any entity ingame. But all recipes should still be loadable from the
// database, even if they are not referenced by anyone anymore. So we have to
// use Resources.Load. (before we added them to the dict in OnEnable, but that's
// only called for those that are referenced in the game. All others will be
// ignored be Unity.)
//
// A Recipe can be created by right clicking the Resources folder and selecting
// Create -> uMMORPG Recipe. Existing recipes can be found in the Resources
// folder.
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName="New Recipe", menuName="uMMORPG Recipe", order=999)]
public class RecipeTemplate : ScriptableObject {
    // fixed ingredient size for all recipes
    public static int recipeSize = 6;

    // ingredients and result
    public List<ItemTemplate> ingredients = new List<ItemTemplate>(6);
    public ItemTemplate result;

    // check if the list of items works for this recipe. the list shouldn't
    // contain 'null'.
    public bool CanCraftWith(List<ItemTemplate> items) {
        // items list should not be touched, since it's often used to check more
        // than one recipe. so let's just create a local copy.
        items = new List<ItemTemplate>(items);
        
        // make sure that we have at least one ingredient
        if (ingredients.Any(it => it != null)) {
            // each ingredient in the list?
            foreach (var ingredient in ingredients)
                if (ingredient != null)
                    if (!items.Remove(ingredient)) return false;
            
            // and nothing else in the list?
            return items.Count == 0;
        } else return false;
    }

    // caching /////////////////////////////////////////////////////////////////
    // we can only use Resources.Load in the main thread. we can't use it when
    // declaring static variables. so we have to use it as soon as 'dict' is
    // accessed for the first time from the main thread.
    static Dictionary<string, RecipeTemplate> cache = null;
    public static Dictionary<string, RecipeTemplate> dict {
        get {
            // load if not loaded yet
            if (cache == null)
                cache = Resources.LoadAll<RecipeTemplate>("").ToDictionary(
                    recipe => recipe.name, recipe => recipe
                );
            return cache;
        }
    }

    // validation //////////////////////////////////////////////////////////////
    void OnValidate() {
        // force list size
        // -> add if too few
        for (int i = ingredients.Count; i < recipeSize; ++i)
            ingredients.Add(null);

        // -> remove if too many
        for (int i = recipeSize; i < ingredients.Count; ++i)
            ingredients.RemoveAt(i);
    }
}
