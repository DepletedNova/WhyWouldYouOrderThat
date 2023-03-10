using UnityEngine.VFX;

namespace WWYOT.Items
{
    internal class EverythingStewServing : CustomItem
    {
        public override string UniqueNameID => "Everything Stew Serving";
        public override ItemCategory ItemCategory => ItemCategory.Generic;
        public override ItemStorage ItemStorageFlags => ItemStorage.StackableFood;
        public override GameObject Prefab => Main.Bundle.LoadAsset<GameObject>("Everything Serving");
        public override ItemValue ItemValue => ItemValue.MediumLarge;
        public override string ColourBlindTag => "KSS";

        public override void OnRegister(GameDataObject gdo)
        {
            Prefab.GetChild("Bowl").ApplyMaterials("Plate");

            var preset = Prefab.GetChild("Stew");
            preset.ApplyMaterials("Soup - Meat");
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
