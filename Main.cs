global using KitchenData;
global using KitchenLib;
global using KitchenLib.Customs;
global using KitchenLib.References;
global using KitchenLib.Utils;
global using System.Collections.Generic;
global using UnityEngine;
global using static KitchenLib.Utils.GDOUtils;
using HarmonyLib;
using Kitchen;
using KitchenLib.Event;
using KitchenMods;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEngine.VFX;
using WWYOT.Dishes;
using WWYOT.Items;

namespace WWYOT
{
    public class Main : BaseMod
    {
        public const string GUID = "nova.wwyot";
        public const string VERSION = "1.0.4";

        public Main() : base(GUID, "Why Would You Order That?", "Depleted Supernova#1957", VERSION, ">=1.1.0", Assembly.GetExecutingAssembly()) { }

        public static AssetBundle Bundle;

        private void AddGDOs()
        {
            // Steak
            AddGameDataObject<BurntSteakDish>();
            AddGameDataObject<RawSteakDish>();

            // Stew
            AddGameDataObject<EverythingStewDish>();
            AddGameDataObject<UncookedEverythingStew>();
            AddGameDataObject<CookedEverythingStew>();
            AddGameDataObject<EverythingStewServing>();
        }

        private static List<int> GetQueue() => new()
        {
            // Steak
            ItemReferences.Meat,
            ItemReferences.SteakRare,
            ItemReferences.SteakMedium,
            ItemReferences.SteakWelldone,
            ItemReferences.SteakBurned,
            ItemReferences.SteakPlated,
            DishReferences.Steak,

            ItemReferences.MeatThin,
            ItemReferences.ThinSteakRare,
            ItemReferences.ThinSteakMedium,
            ItemReferences.ThinSteakWelldone,
            ItemReferences.ThinSteakBurned,
            ItemReferences.ThinSteakPlated,
            DishReferences.ThinSteaks,

            ItemReferences.MeatThick,
            ItemReferences.ThickSteakRare,
            ItemReferences.ThickSteakMedium,
            ItemReferences.ThickSteakWelldone,
            ItemReferences.ThickSteakBurned,
            ItemReferences.ThickSteakPlated,
            DishReferences.ThickSteaks,

            ItemReferences.MeatBoned,
            ItemReferences.BonedSteakRare,
            ItemReferences.BonedSteakMedium,
            ItemReferences.BonedSteakWelldone,
            ItemReferences.BonedSteakBurned,
            ItemReferences.BonedSteakPlated,
            DishReferences.BonedSteaks,
        };

        protected override void OnPostActivate(Mod mod)
        {
            Bundle = mod.GetPacks<AssetBundleModPack>().SelectMany(e => e.AssetBundles).ToList()[0];

            AddGDOs();

            // Set up everything properly
            Events.BuildGameDataEvent += (_, args) =>
            {
                if (!args.firstBuild)
                    return;

                Steaks.SetupSteakPrefabs(args.gamedata);
                SetupStew(args.gamedata);
            };
        }

        [HarmonyPatch(typeof(GameDataConstructor), nameof(GameDataConstructor.BuildGameData))]
        class Modify_GameDataConstructor_Patch
        {
            private static bool firstRun = true;
            [HarmonyPrefix]
            internal static void Prefix(GameDataConstructor __instance)
            {
                if (!firstRun)
                    return;
                firstRun = true;

                // Add to query
                var queue = GetQueue();
                Dictionary<int, GameDataObject> gdos = new();
                foreach (var gdo in __instance.GameDataObjects)
                {
                    if (!queue.Contains(gdo.ID))
                        continue;
                    gdos.Add(gdo.ID, gdo);
                }

                // Modify
                Steaks.SetupSteakGDOs(gdos);
            }
        }

        #region Stew
        private static FieldInfo ProviderItemField = ReflectionUtils.GetField<CItemProvider>("Item");

        internal static List<int> ValidStewItem = new();
        internal void SetupStew(GameData gameData)
        {
            ItemGroup stew = GetCastedGDO<ItemGroup, UncookedEverythingStew>();
            var stewView = stew.Prefab.GetComponent<UncookedEverythingStew.StewItemGroupView>();

            List<Item> items = new();
            List<Item> checkedItems = new()
            {
                GetExistingGDO(ItemReferences.Meat) as Item,
                GetExistingGDO(ItemReferences.MeatChopped) as Item,
                GetExistingGDO(ItemReferences.Potato) as Item,
                GetExistingGDO(ItemReferences.PotatoChopped) as Item,
            };

            // Setup array
            List<Appliance> appliances = new();
            foreach (var gdo in gameData.Objects)
                if (gdo.Value is Appliance appliance)
                    appliances.Add(appliance);
            foreach (var cgdo in CustomGDO.GDOs)
                if (cgdo.Value.GameDataObject is Appliance appliance)
                    appliances.Add(appliance);

            foreach (var appliance in appliances)
            {
                var providerCheck = appliance.Properties.OfType<CItemProvider>();
                if (providerCheck.IsNullOrEmpty())
                    continue;

                var provider = providerCheck.FirstOrDefault();
                if (!gameData.TryGet<Item>((int)ProviderItemField.GetValue(provider), out var item, false))
                    continue;

                checkedItems.Add(item);

                RecursivelyCheckItem(item, ref checkedItems, ref items, ref stewView);
            }

            stew.DerivedSets[2].Items.AddRange(items);
        }
        private void RecursivelyCheckItem(Item item, ref List<Item> checkedItems, ref List<Item> items, ref UncookedEverythingStew.StewItemGroupView view)
        {
            var itemsToProcess = new List<Item>();
            foreach (var process in item.DerivedProcesses)
            {
                if (process.Process.ID != ProcessReferences.Chop && process.Process.ID != ProcessReferences.Knead)
                    continue;

                if (process.Result.IsIndisposable || checkedItems.Contains(process.Result))
                    continue;

                checkedItems.Add(process.Result);
                itemsToProcess.Add(process.Result);
                items.Add(process.Result);
                view.AddItem(process.Result);
                RecursivelyCheckItem(process.Result, ref checkedItems, ref items, ref view);
            }
            if (item.SplitSubItem is Item subItem && !subItem.IsIndisposable && !checkedItems.Contains(subItem))
            {
                checkedItems.Add(subItem);
                itemsToProcess.Add(subItem);
                items.Add(subItem);
                view.AddItem(subItem);
                RecursivelyCheckItem(subItem, ref checkedItems, ref items, ref view);
            }
        }
        #endregion

        #region Steam
        private static VisualEffectAsset _steamEffect;
        internal static VisualEffectAsset SteamEffect
        {
            get
            {
                if (_steamEffect == null)
                {
                    foreach (VisualEffectAsset asset in Resources.FindObjectsOfTypeAll<VisualEffectAsset>())
                    {
                        if (!asset.name.ToLower().Contains("steam"))
                            continue;

                        _steamEffect = asset;
                        break;
                    }
                }

                return _steamEffect;
            }
        }
        #endregion
    }

    #region ItemGroupView
    internal class ComponentAccesserUtil : ItemGroupView
    {
        private static FieldInfo componentGroupField = ReflectionUtils.GetField<ItemGroupView>("ComponentGroups");

        public static void AddComponent(ItemGroupView viewToAddTo, params (Item item, GameObject gameObject)[] addedGroups)
        {
            List<ComponentGroup> componentGroups = componentGroupField.GetValue(viewToAddTo) as List<ComponentGroup>;
            foreach (var group in addedGroups)
            {
                componentGroups.Add(new()
                {
                    Item = group.item,
                    GameObject = group.gameObject
                });
            }
            componentGroupField.SetValue(viewToAddTo, componentGroups);
        }
    }
    #endregion
}
