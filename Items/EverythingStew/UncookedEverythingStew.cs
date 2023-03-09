using Kitchen;
using KitchenData;
using UnityEngine;
using WWYOT.Items.EverythingStew;

namespace WWYOT.Items
{
    public class UncookedEverythingStew : CustomItemGroup<UncookedEverythingStew.StewItemGroupView>
    {
        internal static int ItemID { get; private set; }

        public override string UniqueNameID => "Uncooked Everything Stew Pot";
        public override ItemCategory ItemCategory => ItemCategory.Generic;
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override GameObject Prefab => Main.Bundle.LoadAsset<GameObject>("Uncooked Everything");
        public override Item DisposesTo => GetExistingGDO(ItemReferences.Pot) as Item;
        public override List<Item.ItemProcess> Processes => new()
        {
            new()
            {
                Duration = 10,
                Process = GetExistingGDO(ProcessReferences.Cook) as Process,
                Result = GetCastedGDO<Item, CookedEverythingStew>()
            }
        };
        public override List<ItemGroup.ItemSet> Sets => new()
        {
            new()
            {
                IsMandatory = true,
                Items = new()
                {
                    GetExistingGDO(ItemReferences.Pot) as Item,
                    GetExistingGDO(ItemReferences.Water) as Item
                },
                Max = 2,
                Min = 2,
            },
            new()
            {
                Items = new()
                {
                    GetExistingGDO(ItemReferences.MeatChopped) as Item,
                    GetExistingGDO(ItemReferences.PotatoChopped) as Item
                },
                Max = 2,
                Min = 2
            },
            new()
            {
                Items = new(),
                Max = 999,
                Min = 999
            }
        };

        public override void OnRegister(GameDataObject gdo)
        {
            var pot = Prefab.GetChild("Pot");
            pot.GetChild("Cylinder").ApplyMaterials("Metal");
            pot.GetChild("Cylinder.003").ApplyMaterials("Cylinder.003", "Metal Dark");

            Prefab.GetChild("Water").ApplyMaterials("Water");

            var preset = Prefab.GetChild("Preset");
            preset.GetChild("Meat").ApplyMaterials("Raw", "Raw Fat");
            preset.GetChild("Potato").ApplyMaterials("Raw Potato");

            Prefab.GetComponent<StewItemGroupView>().SetupDefault();
            ItemID = gdo.ID;


        }

        public class StewItemGroupView : ItemGroupView
        {
            public void SetupDefault()
            {
                var preset = gameObject.GetChild("Preset");

                ComponentGroups = new()
                {
                    new()
                    {
                        Item = GetExistingGDO(ItemReferences.MeatChopped) as Item,
                        GameObject = preset.GetChild("Meat"),
                    },
                    new()
                    {
                        Item = GetExistingGDO(ItemReferences.PotatoChopped) as Item,
                        GameObject = preset.GetChild("Potato")
                    }
                };
            }

            public void AddItem(Item item)
            {
                ComponentGroups.Add(new()
                {
                    Item = item,
                    GameObject = CloneItem(item.Prefab)
            });
            }
            private GameObject CloneItem(GameObject og)
            {
                GameObject addedPrefab = Instantiate(og);
                Transform addedTransform = addedPrefab.transform;
                addedTransform.SetParent(gameObject.GetChild("Chunks").transform, false);
                addedTransform.localPosition = new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                addedTransform.localRotation = Quaternion.Euler(Random.Range(-20, 20), Random.Range(-180, 180), Random.Range(-20, 20));
                addedTransform.localScale = Vector3.one * 0.9f;
                return addedPrefab;
            }
        }
    }
}
