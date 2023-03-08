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
                    if (ValidStewItem.Contains(ingredient.ID))
                    {
                        AvailableIngredientCount++;
                    }
                }
            }
        }

        internal static List<int> ValidStewItem = new();
        internal static void SetupStew(GameData gameData)
        {
            ItemGroup stew = GetCastedGDO<ItemGroup, UncookedEverythingStew>();
            var stewView = stew.Prefab.GetComponent<UncookedEverythingStew.StewItemGroupView>();

            List<Item> items = new();
            List<Item> checkedItems = new()
            {
                GetExistingGDO(ItemReferences.Meat) as Item,
                GetExistingGDO(ItemReferences.Potato) as Item
            };

            foreach (ICard card in gameData.GetCards())
            {
                if (!(card is Dish))
                    continue;

                SetupStewDish(ref stewView, ref checkedItems, ref items, card as Dish);
            }

            foreach (var gdo in CustomGDO.GDOs)
            {
                if (!(gdo.Value.GameDataObject is Dish))
                    continue;

                SetupStewDish(ref stewView, ref checkedItems, ref items, gdo.Value.GameDataObject as Dish);
            }

            ItemGroup.ItemSet set = new()
            {
                Items = items,
                Max = items.Count,
                Min = items.Count,
            };
            stew.DerivedSets.Add(set);
        }

        private static void SetupStewDish(ref UncookedEverythingStew.StewItemGroupView view, ref List<Item> checkedItems, ref List<Item> items, Dish dish)
        {
            foreach (var item in dish.MinimumIngredients)
            {
                if (item.IsIndisposable || checkedItems.Contains(item)) continue;

                checkedItems.Add(item);

                foreach (var process in item.DerivedProcesses)
                {
                    if (process.Process.ID != ProcessReferences.Chop)
                        continue;

                    items.Add(process.Result);
                    ValidStewItem.Add(item.ID);

                    view.AddItem(process.Result);

                    break;
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
        [HarmonyPrefix]
        static void CreateItemGroup_Prefix(EntityContext ctx, int item_id, ItemList components, ref bool is_partial)
        {
            if (item_id == UncookedEverythingStew.ItemID && components.Count - 4 >= StewIngredientCollectionSystem.AvailableIngredientCount)
            {
                is_partial = false;
            }
        }

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
