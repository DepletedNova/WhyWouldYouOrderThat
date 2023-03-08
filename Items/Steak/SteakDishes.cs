namespace WWYOT.Items
{
    internal class BurntSteakDish : SteakDish
    {
        internal override bool IsBurnt => true;

        public override Dictionary<Locale, string> Recipe => new()
        {
            { Locale.English, "Burn the meat and serve." }
        };

        public override List<(Locale, UnlockInfo)> InfoList => new()
        {
            (Locale.English, LocalisationUtils.CreateUnlockInfo("Burnt Connoisseur", "Customs will order Burnt Steaks", "That definitely isn't healthy."))
        };
    }

    internal class RawSteakDish : SteakDish
    {
        internal override bool IsBurnt => false;

        public override Dictionary<Locale, string> Recipe => new()
        {
            { Locale.English, "Serve raw meat as is!" }
        };

        public override List<(Locale, UnlockInfo)> InfoList => new()
        {
            (Locale.English, LocalisationUtils.CreateUnlockInfo("Omophagists", "Customers will order Blue Steaks.", "I worry for them."))
        };
    }
}
