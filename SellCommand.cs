using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMarket.Controllers;

namespace SMarket;

public class SellCommand : IRocketCommand
{
    #region IRocketCommand
    public AllowedCaller AllowedCaller { get; } = AllowedCaller.Player;

    public string Name { get; } = "sell";

    public string Help { get; }

    public string Syntax { get; }

    public List<string> Aliases { get; } = new() { };

    List<string> permissions;
    public List<string> Permissions => permissions ??= Aliases.Prepend(Name).ToList();

    public async void Execute(IRocketPlayer caller, string[] command)
    {
        await ExecuteAsync(caller as UP, command);
    }
    #endregion
    public Task ExecuteAsync(UP up, string[] command) => up.GetComponent<SellController>().OpenStorage();
}
