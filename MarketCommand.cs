using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMarket.Controllers;

namespace SMarket;

public class MarketCommand : IRocketCommand
{
    #region IRocketCommand
    public AllowedCaller AllowedCaller { get; } = AllowedCaller.Player;

    public string Name { get; } = "market";

    public string Help { get; }

    public string Syntax { get; }

    public List<string> Aliases { get; } = new() { };

    List<string> permissions;
    public List<string> Permissions => permissions ??= Aliases.Prepend(Name).ToList();
    #endregion

    public async void Execute(IRocketPlayer caller, string[] command)
    {
        var up = (UP)caller;
        up.GetComponent<MarketController>().Show();
    }
}
