using SMarket.Views;

namespace SMarket.Models;

public class MarketServerItem : MarketItemView
{
    [XmlArrayItem("Command")]
    public List<string> Commands { get; set; } = new List<string>();

    public override string Seller => TranslateServer();

    public override void OnPurchased(UnturnedPlayer buyer)
    {
        var console = new ConsolePlayer();
        foreach (var command in Commands)
            try {
                R.Commands.Execute(console, string.Format(command, buyer.CSteamID));
            }
            catch { }
    }
}
