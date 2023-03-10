using HarmonyLib;
using Kitchen;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;

namespace WWYOT.Items.EverythingStew
{
    public class StewIngredientCollectionSystem : StartOfDaySystem, IModSystem
    {
        internal static int AvailableIngredientCount = 0;

        private EntityQuery CurrentUnlocks;

        protected override void Initialise()
        {
            base.Initialise();
            CurrentUnlocks = GetEntityQuery(new QueryHelper().All(typeof(CProgressionUnlock)));
        }

        protected override void OnUpdate()
        {
            using var unlocks = CurrentUnlocks.ToComponentDataArray<CProgressionUnlock>(Allocator.Temp);

            AvailableIngredientCount = 0;
            foreach (CProgressionUnlock unlock in unlocks)
            {
                if (!GameData.Main.TryGet<Dish>(unlock.ID, out var dish, false))
                    continue;

                foreach (var ingredient in dish.MinimumIngredients)
                {
                    if (Main.ValidStewItem.Contains(ingredient.ID))
                    {
                        AvailableIngredientCount++;
                    }
                }
            }
        }
    }

    public class PortionCountFromStewIngredientsSystem : DaySystem, IModSystem
    {
        private EntityQuery StewInstances;
        protected override void Initialise()
        {
            base.Initialise();
            StewInstances = GetEntityQuery(new QueryHelper().All(typeof(CHasIngredientsForStew), typeof(CSplittableItem), typeof(CItem)));
        }

        protected override void OnUpdate()
        {
            using var entities = StewInstances.ToEntityArray(Allocator.Temp);

            foreach (var entity in entities)
            {
                CSplittableItem splittable = GetComponent<CSplittableItem>(entity);
                CHasIngredientsForStew ingredients = GetComponent<CHasIngredientsForStew>(entity);

                if (splittable.TotalCount == ingredients.ingredients)
                    continue;
                int amount = ingredients.ingredients * 2 + 2;
                splittable.TotalCount = amount;
                splittable.RemainingCount = amount;

                Set(entity, splittable);

                EntityManager.RemoveComponent<CHasIngredientsForStew>(entity);
            }
        }

    }

    [HarmonyPatch(typeof(ItemExtensions))]
    internal class StewItemGroup_Patch
    {
        [HarmonyPatch(nameof(ItemExtensions.CreateItemGroup))]
        [HarmonyPostfix]
        static void CreateItemGroup_Postfix(ref Entity __result, EntityContext ctx, int item_id, ItemList components, bool is_partial)
        {
            if (item_id == UncookedEverythingStew.ItemID && !is_partial)
            {
                ctx.Set<CHasIngredientsForStew>(__result, new()
                {
                    ingredients = components.Count - 4,
                });
            }
        }
    }

    internal struct CHasIngredientsForStew : IComponentData, IModComponent {
        public int ingredients;
    }
}
