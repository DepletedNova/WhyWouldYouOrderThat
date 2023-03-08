using Kitchen;
using KitchenLib.Colorblind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;

namespace WWYOT.Items
{
    internal static class Steaks
    {
        internal static void SetupSteakGDOs(Dictionary<int, GameDataObject> GDOs)
        {
            // Basic
            ModifySteakGDO(
                GDOs[ItemReferences.Meat] as Item, GDOs[ItemReferences.SteakRare] as Item, 
                GDOs[ItemReferences.SteakMedium] as Item, GDOs[ItemReferences.SteakWelldone] as Item, 
                GDOs[ItemReferences.SteakBurned] as Item, GDOs[ItemReferences.SteakPlated] as ItemGroup, 
                GDOs[DishReferences.Steak] as Dish);

            // Thin
            ModifySteakGDO(
                GDOs[ItemReferences.MeatThin] as Item, GDOs[ItemReferences.ThinSteakRare] as Item,
                GDOs[ItemReferences.ThinSteakMedium] as Item, GDOs[ItemReferences.ThinSteakWelldone] as Item,
                GDOs[ItemReferences.ThinSteakBurned] as Item, GDOs[ItemReferences.ThinSteakPlated] as ItemGroup,
                GDOs[DishReferences.ThinSteaks] as Dish);

            // Thick
            ModifySteakGDO(
                GDOs[ItemReferences.MeatThick] as Item, GDOs[ItemReferences.ThickSteakRare] as Item,
                GDOs[ItemReferences.ThickSteakMedium] as Item, GDOs[ItemReferences.ThickSteakWelldone] as Item,
                GDOs[ItemReferences.ThickSteakBurned] as Item, GDOs[ItemReferences.ThickSteakPlated] as ItemGroup,
                GDOs[DishReferences.ThickSteaks] as Dish);

            // Boned
            ModifySteakGDO(
                GDOs[ItemReferences.MeatBoned] as Item, GDOs[ItemReferences.BonedSteakRare] as Item,
                GDOs[ItemReferences.BonedSteakMedium] as Item, GDOs[ItemReferences.BonedSteakWelldone] as Item,
                GDOs[ItemReferences.BonedSteakBurned] as Item, GDOs[ItemReferences.BonedSteakPlated] as ItemGroup,
                GDOs[DishReferences.BonedSteaks] as Dish);
        }

        internal static void SetupSteakPrefabs(GameData gameData)
        {
            // Basic
            var basic = ModifySteakPrefab(gameData, ItemReferences.Meat, ItemReferences.SteakBurned, ItemReferences.SteakPlated, "Steak/Burned Steak", "Rare Steak");
            basic.GetChild("Meat").ApplyMaterials("Raw Fat", "Raw Fat", "Raw");

            // Thin
            var thin = ModifySteakPrefab(gameData, ItemReferences.MeatThin, ItemReferences.ThinSteakBurned, ItemReferences.ThinSteakPlated,
                "Thin Steak/Thin Steak - 4 Burned", "Thin Steak - 5 Raw").GetChild("Meat - Thin").GetChild("Thin Meat");
            thin.GetChild("Fat").ApplyMaterials("Raw Fat");
            thin.GetChild("Meat").ApplyMaterials("Raw");

            // Thick
            var thick = ModifySteakPrefab(gameData, ItemReferences.MeatThick, ItemReferences.ThickSteakBurned, ItemReferences.ThickSteakPlated,
                "Thick Steak/Thick Steak - 4 Burned", "Thick Steak - 5 Raw").GetChild("Meat - Thick").GetChild("Thick Meat");
            thick.GetChild("Plane.001").ApplyMaterials("Raw Fat");
            thick.GetChild("Plane").ApplyMaterials("Raw");

            // Boned
            var steakPlated = gameData.Get<ItemGroup>(ItemReferences.SteakPlated).Prefab;
            var boned = ModifySteakPrefab(gameData, ItemReferences.MeatBoned, ItemReferences.BonedSteakBurned, ItemReferences.BonedSteakPlated,
                "Boned Steak/Boned Steak - 4 Burned", "Boned Steak - 5 Raw").GetChild("Boned Meat");
            boned.GetChild("Bone").ApplyMaterials("Raw Fat");
            boned.GetChild("Meat").ApplyMaterials("Raw");
            ComponentAccesserUtil.AddComponent(steakPlated.GetComponent<ItemGroupView>(),
                (gameData.Get<Item>(ItemReferences.BonedSteakBurned), GameObjectUtils.GetChildObject(steakPlated, "Boned Steak/Boned Steak - 4 Burned")));
        }
        
        // GameData, Item IDs, Path to Burnt, Name of Raw
        private static GameObject ModifySteakPrefab(GameData gameData, int meat, int burnt, int plated, string path, string name)
        {
            var meatItem = gameData.Get<Item>(meat);
            var burntItem = gameData.Get<Item>(burnt);
            var platedItem = gameData.Get<ItemGroup>(plated);

            // Modify Prefabs
            ColorblindUtils.getTextMeshProFromClonedObject(ColorblindUtils.cloneColourBlindObjectAndAddToItem(meatItem)).text = "Bl";
            ColorblindUtils.getTextMeshProFromClonedObject(ColorblindUtils.cloneColourBlindObjectAndAddToItem(burntItem)).text = "Bu";

            var burntGameObject = GameObjectUtils.GetChildObject(platedItem.Prefab, path);
            var rawGameObject = burntGameObject.Clone(name);

            var burntColorBlind = ColorblindUtils.cloneColourBlindObjectAndAddToItem(platedItem);
            burntColorBlind.transform.SetParent(burntGameObject.transform, false);
            ColorblindUtils.getTextMeshProFromClonedObject(burntColorBlind).text = "Bu";

            var rawColorBlind = ColorblindUtils.cloneColourBlindObjectAndAddToItem(platedItem);
            rawColorBlind.transform.SetParent(rawGameObject.transform, false);
            ColorblindUtils.getTextMeshProFromClonedObject(rawColorBlind).text = "Bl";

            ComponentAccesserUtil.AddComponent(platedItem.Prefab.GetComponent<ItemGroupView>(), (meatItem, rawGameObject));

            return rawGameObject;
        }

        private static FieldInfo SetsField = ReflectionUtils.GetField<ItemGroup>("Sets");
        private static FieldInfo IngredientsUnlockField = ReflectionUtils.GetField<Dish>("IngredientsUnlocks");
        private static void ModifySteakGDO(Item meat, Item rare, Item medium, Item welldone, Item burnt, ItemGroup plated, Dish dish)
        {
            // Sets
            var sets = SetsField.GetValue(plated) as List<ItemGroup.ItemSet>;
            var setIndex = sets.FindIndex(set => set.Items.Contains(rare));
            sets[setIndex] = new()
            {
                Items = new()
                {
                    meat,
                    rare,
                    medium,
                    welldone,
                    burnt
                },
                Min = 1,
                Max = 1,
                RequiresUnlock = true
            };
            SetsField.SetValue(plated, sets);

            // Ingredients
            HashSet<Dish.IngredientUnlock> ingredients = new()
            {
                new()
                {
                    Ingredient = rare,
                    MenuItem = plated
                },
                new()
                {
                    Ingredient = medium,
                    MenuItem = plated
                },
                new()
                {
                    Ingredient = welldone,
                    MenuItem = plated
                },
            };
            IngredientsUnlockField.SetValue(dish, ingredients);
        }
    }

    internal abstract class SteakDish : CustomDish
    {
        internal abstract bool IsBurnt { get; }

        public override string UniqueNameID => $"{(IsBurnt ? "Burnt" : "Raw")} Steak Dish";
        public override DishType Type => DishType.Extra;
        public override CardType CardType => CardType.Default;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.SmallDecrease;

        public override List<Unlock> HardcodedRequirements => new()
        {
            GetExistingGDO(DishReferences.Steak) as Unlock
        };

        public override HashSet<Dish.IngredientUnlock> IngredientsUnlocks => new()
        {
            new()
            {
                Ingredient = GetExistingGDO(IsBurnt ? ItemReferences.SteakBurned : ItemReferences.Meat) as Item,
                MenuItem = GetExistingGDO(ItemReferences.SteakPlated) as ItemGroup
            },
            new()
            {
                Ingredient = GetExistingGDO(IsBurnt ? ItemReferences.ThinSteakBurned : ItemReferences.MeatThin) as Item,
                MenuItem = GetExistingGDO(ItemReferences.ThinSteakPlated) as ItemGroup
            },
            new()
            {
                Ingredient = GetExistingGDO(IsBurnt ? ItemReferences.ThickSteakBurned : ItemReferences.MeatThick) as Item, 
                MenuItem = GetExistingGDO(ItemReferences.ThickSteakPlated) as ItemGroup
            },
            new()
            {
                Ingredient = GetExistingGDO(IsBurnt ? ItemReferences.BonedSteakBurned : ItemReferences.MeatBoned) as Item, 
                MenuItem = GetExistingGDO(ItemReferences.BonedSteakPlated) as ItemGroup
            }
        };
    }
}
