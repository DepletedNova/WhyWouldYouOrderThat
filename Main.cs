global using KitchenLib.References;
global using KitchenLib.Customs;
global using KitchenLib.Utils;
global using KitchenLib;
global using KitchenData;
global using System.Collections.Generic;
global using UnityEngine;

global using static KitchenLib.Utils.GDOUtils;

using KitchenMods;
using System.Linq;
using System.Reflection;
using WWYOT.Items;
using KitchenLib.Event;
using UnityEngine.VFX;
using KitchenLib.Colorblind;
using Kitchen;
using System;
using WWYOT.Items.EverythingStew;
using System.Diagnostics;
using HarmonyLib;

namespace WWYOT
{
    public class Main : BaseMod
    {
        public const string GUID = "nova.wwyot";
        public const string VERSION = "1.0.0";

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
                StewIngredientCollectionSystem.SetupStew(args.gamedata);
            };
        }

        [HarmonyPatch(typeof(GameDataConstructor), nameof(GameDataConstructor.BuildGameData))]
        class Modify_GameDataConstructor_Patch
        {
            [HarmonyPrefix]
            internal static void Prefix(GameDataConstructor __instance)
            {
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

    }

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
}
