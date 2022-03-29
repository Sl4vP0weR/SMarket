global using static SMarket.Translations;

namespace SMarket
{
    // Don't rename this class, it will be used in analyzer.
    public static partial class Translations
    {
        #region Base
        public static char[] TranslationKeyTrimCharacters = new[] { '_' };
        /// <summary>
        /// Retrieves values from <see cref="Translations"/> type. [Only "<see langword="public"/> <see langword="static"/> <see langword="readonly"/> <see langword="string"/>" or 
        /// "<see langword="public"/> <see langword="const"/> <see langword="string"/>" fields]
        /// </summary>
        public static TranslationList DefaultTranslationList
        {
            get
            {
                var translations = new TranslationList();
                translations.AddRange(
                typeof(Translations).GetFields(BindingFlags.Static | BindingFlags.Public)
                .Where(x => (x.IsStatic && x.IsInitOnly) || x.IsLiteral)
                .Select(x =>
                    new TranslationListEntry(x.Name.Trim(TranslationKeyTrimCharacters), (x.IsLiteral ? x.GetRawConstantValue() : x.GetValue(null)).ToString())
                ));
                return translations;
            }
        }
        /// <summary>
        /// This method is important for analyzer!
        /// </summary>
        public static string Translate(string translationKey, params object[] arguments) => inst.Translate(translationKey, arguments);
        #endregion
        // You can write static/const string fields(translations) here. By default _ will be trimmed
        public const string
            Hello = "Hello from RocketMod.Modern!",
            ServerTabName = "Black market",
            PlayersTabName = "Flea market",
            Purchased = "You've purchased {0} for {1}$",
            NotWhitelisted = "Item {0} isn't whitelisted for market.",
            InvalidCost = "Invalid cost. [Min: {0} | Max: {1}]",
            SomethingWrong = "Something went wrong...",
            ItemPlacedToPlayers = "Item placed on flea for {0}$.",
            ItemSoldToServer = "Item sold to server for {0}$",
            SellToServer = "Sell to server for {0}$",
            SellToPlayers = "Put it on flea",
            Server = "Server",
            Price = "${0}",
            Amount = "{0}x",
            Pages = "{0}/{1}",
            Total = "Total: {0} items";
    }
}