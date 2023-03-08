using UnityEngine.VFX;
using WWYOT.Items.EverythingStew;

namespace WWYOT.Items
{
    internal class CookedEverythingStew : CustomItem
    {
        public override string UniqueNameID => "Cooked Everything Stew Pot";
        public override ItemCategory ItemCategory => ItemCategory.Generic;
        public override ItemStorage ItemStorageFlags => ItemStorage.None;
        public override GameObject Prefab => Main.Bundle.LoadAsset<GameObject>("Cooked Everything");
        public override Item DisposesTo => GetExistingGDO(ItemReferences.Pot) as Item;
        public override Item SplitSubItem => GetCastedGDO<Item, EverythingStewServing>();
        public override float SplitSpeed => 1;
        public override int SplitCount => 99;
        public override List<Item> SplitDepletedItems => new() { GetExistingGDO(ItemReferences.Pot) as Item };

        public override void OnRegister(GameDataObject gdo)
        {
            var pot = Prefab.GetChild("Pot");
            pot.GetChild("Cylinder").ApplyMaterials("Metal");
            pot.GetChild("Cylinder.003").ApplyMaterials("Cylinder.003", "Metal Dark");

            Prefab.GetChild("Water").ApplyMaterials("Soup - Meat");

            var preset = Prefab.GetChild("Preset");
            preset.GetChild("Meat").ApplyMaterials("Well-done", "Well-done Fat");
            preset.GetChild("Potato").ApplyMaterials("Cooked Potato");
            preset.GetChild("Carrot").ApplyMaterials("Carrot - Cooked");
            preset.GetChild("Tomato").ApplyMaterials("Tomato Soup");
            preset.GetChild("Broccoli").ApplyMaterials("Cooked Broccoli");
            preset.GetChild("Mushroom").ApplyMaterials("Mushroom Cooked");
            preset.GetChild("Cheese").ApplyMaterials("Cheese - Pizza");

            Prefab.GetChild("Steam").GetComponent<VisualEffect>().visualEffectAsset = Main.SteamEffect;
        }
    }
}
