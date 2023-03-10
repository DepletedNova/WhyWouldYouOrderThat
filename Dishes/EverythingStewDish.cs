using WWYOT.Items;

namespace WWYOT.Dishes
{
    public class EverythingStewDish : CustomDish
    {
        public override string UniqueNameID => "Everything Stew Dish";
        public override DishCustomerChange CustomerMultiplier => DishCustomerChange.LargeDecrease;
        public override UnlockGroup UnlockGroup => UnlockGroup.Dish;
        public override Unlock.RewardLevel ExpReward => Unlock.RewardLevel.Medium;
        public override DishType Type => DishType.Starter;
        public override List<(Locale, UnlockInfo)> InfoList => new()
        {
            (Locale.English, LocalisationUtils.CreateUnlockInfo("Kitchen Sink Stew", "A little bit of everything in a stew!", "That can't be good"))
        };
        public override Dictionary<Locale, string> Recipe => new()
        {
            { Locale.English, "Take a pot and fill it with water. Chop both a potato and meat then add them both. " +
                "If an ingredient in the kitchen can be chopped or kneaded then do so and add it. Cooked, portion, and then serve." }
        };
        public override HashSet<Item> MinimumIngredients => new()
        {
            GetExistingGDO(ItemReferences.Potato) as Item,
            GetExistingGDO(ItemReferences.Meat) as Item,
            GetExistingGDO(ItemReferences.Pot) as Item,
            GetExistingGDO(ItemReferences.Water) as Item,
        };
        public override HashSet<Process> RequiredProcesses => new()
        {
            GetExistingGDO(ProcessReferences.Cook) as Process,
            GetExistingGDO(ProcessReferences.Chop) as Process,
        };
        public override List<Dish.MenuItem> ResultingMenuItems => new()
        {
            new()
            {
                Item = GetCastedGDO<ItemGroup, EverythingStewServing>(),
                Phase = MenuPhase.Starter,
                Weight = 1f
            }
        };
    }
}
