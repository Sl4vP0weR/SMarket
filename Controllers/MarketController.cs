using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using fr34kyn01535.Uconomy;
using SmartUI;
using SmartUI.Controllers;
using SmartUI.Elements;

namespace SMarket.Controllers;

public partial class MarketController : ControllerBase
{
    protected override void Load()
    {
    }
    protected override void Unload()
    {
        SetActive(false);
    }
    public override ushort ID => conf.Market.ID;
    public override short Key => conf.Market.Key;
    public override EPluginWidgetFlags Widgets => EPluginWidgetFlags.Modal;

    protected override List<Element> DefaultElements => new()
    {
        new(conf.Market.Close, new Button(Hide)),
        new(conf.Market.Search.Button, new Button(Search)),
        new(conf.Market.Tabs.Players, new Button(OpenPlayersTab)),
        new(conf.Market.Tabs.Server, new Button(OpenServerTab)),
        new(conf.Market.Tabs.Server, new Button(OpenServerTab)),
        new(conf.Market.Total, new Text()),
        new(conf.Market.Pagination.Page, new Text()),
        new(conf.Market.Pagination.Next, new Button(NextPage)),
        new(conf.Market.Pagination.Previous, new Button(PreviousPage)),
        new(conf.Market.Search.Text, new Field())
    };

    public TabContext Context { get; private set; }

    public async Task UpdateContext(TabContext newContext!!)
    {
        Context = newContext;
        await Context.Render();
        GetElement<Text>(conf.Market.Total).UpdateText(this, TranslateTotal(Context.Items.Count));
        var pagesCount = Context.PagesCount;
        var currentPage = Context.CurrentPageIndex + 1;
        GetElement<Text>(conf.Market.Pagination.Page).UpdateText(this, TranslatePages(Mathf.Min(currentPage, pagesCount), pagesCount));
    }

    public Task OpenPlayersTab() => UpdateContext(new PlayersTabContext(this));
    public Task OpenServerTab() => UpdateContext(new ServerTabContext(this));

    public Task NextPage() => Context.OpenNextPage();
    public Task PreviousPage() => Context.OpenPreviousPage();

    public override Task OnButtonClicked(string name)
    {
        var ids = name.TryGetIds(conf.Market.Row.Buy);
        if (ids.Any())
        {
            var id = ids.ElementAtOrDefault(0);
            return Context.Buy(id);
        }
        return Task.CompletedTask;
    }

    public Task Search() => Context.Search(GetElement<Field>(conf.Market.Search.Text).Text);

    public void Hide()
    {
        EffectManager.sendUIEffectVisibility(Key, Player.CSteamID, true, conf.Market.Menu, false);
        ApplyWidgets(false);
    }
    public async void Show()
    {
        SetActive(true);
        for (int i = 0; i < 2; i++) // issue with not shown ui
        {
            EffectManager.sendUIEffectVisibility(Key, Player.CSteamID, true, conf.Market.Menu, true);
            await Task.Delay(50);
        }
        ApplyWidgets(true);
    }

    public override Task OnStateUpdated() => OpenPlayersTab();

    protected override void SendEffect() => EffectManager.sendUIEffect(ID, Key, Player.CSteamID, Reliable, TranslatePlayersTabName(), TranslateServerTabName());
}