using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMarket.Entities;
using SMarket.Models;
using SMarket.Views;
using SmartUI.Controllers;
using SmartUI.Elements;

namespace SMarket.Controllers;

public class SellController : ControllerBase
{
    public override ushort ID => conf.Sell.ID;
    public override short Key => conf.Sell.Key;
    public override EPluginWidgetFlags Widgets => EPluginWidgetFlags.Modal;

    protected override void Load()
    {
    }

    protected override async void Unload()
    {
        Hide();
    }

    protected override void SendEffect() =>
        EffectManager.sendUIEffect(ID, Key, Player.CSteamID, Reliable,
            Model.FormatName(Assets.find(EAssetType.EFFECT, Item.item.id) as ItemAsset),
            TranslateSellToPlayers(),
            TranslateSellToServer(Model.ServerCost)
        );

    public PlayerInventory Inventory => Player.Inventory;
    public InteractableStorage Storage { get; private set; }
    public Items StorageItems => Storage.items;

    protected override List<Element> DefaultElements => new()
    {
        new(conf.Sell.Title, new Text()),
        new(conf.Sell.Image, new Image()),
        new(conf.Sell.Close, new Button(Close)),
        new(conf.Sell.SellToServer, new Button(SellToServer)),
        new(conf.Sell.SellToPlayers, new Button(SellToPlayers)),
        new(conf.Sell.Cost, new Field())
    };

    public static Size GetSize(UP player)
    {
        var regex = new Regex(conf.SizePermission.Replace(".", @"\.").Replace("{x}", "([0-9]+)").Replace("{y}", "([0-9]+)"));
        Size? size = R.Permissions.GetPermissions(player)
            .Select(x =>
            {
                try
                {
                    var m = regex.Match(x.Name);
                    if (m.Groups.Count > 2)
                    {
                        var idx = m.Groups.Count - 2;
                        var ms = m.Groups;
                        byte getByte() => byte.Parse(ms[idx++].Value);
                        return new(getByte(), getByte());
                    }
                }
                catch { }
                return conf.DefaultSize;
            })
            .OrderByDescending(x => x.Width + x.Height)
            .FirstOrDefault();
        return size ?? conf.DefaultSize;
    }

    public async Task SellToServer()
    {
        if (!Model.Validate(MarketItem.EType.Server, Model.ServerCost))
            return;

        await RemoveItem();
        uconomy.IncreaseBalance(Player.Id, Model.ServerCost);

        UnturnedChat.Say(Player, TranslateItemSoldToServer(Model.ServerCost), true);
        await OpenStorage();
    }
    public async Task SellToPlayers()
    {
        var costText = GetElement<Field>(conf.Sell.Cost).Text.Replace('.', ',');
        if (!decimal.TryParse(costText, out var cost) || !Model.Validate(MarketItem.EType.Players, cost))
        {
            UnturnedChat.Say(Player, TranslateInvalidCost(Model.MinCost, Model.MaxCost), true);
            return;
        }

        using (var db = Main.DataBase)
        {
            var data = await db.GetOrAddAsync(Player.CSteamID);
            data.SellingItems.Add(new(Item) { Price = cost });
        }

        UnturnedChat.Say(Player, TranslateItemPlacedToPlayers(cost), true);
        await OpenStorage();
    }

    public async Task OpenStorage()
    {
        Hide();
        CloseStorage();
        await UpdateStorage();
        await Task.Delay(10);
        Inventory.openStorage(Storage);
    }
    public void CloseStorage()
    {
        Inventory.closeStorage();
        Inventory.sendStorage();
    }

    static MethodInfo fillSlot = typeof(Items).GetMethod(nameof(fillSlot), BindingFlags.Instance | BindingFlags.NonPublic);
    async Task UpdateStorage()
    {
        await InitStorage();
        using (var db = Main.DataBase)
        {
            var data = await db.GetOrAddAsync(Player.CSteamID);
            foreach (var item in data.SellingItems)
            {
                var jar = item.GetJar();
                if (!StorageItems.checkSpaceEmpty(jar.x, jar.y, jar.size_x, jar.size_y, jar.rot))
                {
                    Player.Inventory.forceAddItem(jar.item, true);
                    await db.RemoveItemAsync(item.Identifier);
                    continue;
                }
                StorageItems.items.Add(jar);
                fillSlot.Invoke(StorageItems, new object[] { jar, true });
            }
        }
    }

    static FieldInfo _items = typeof(InteractableStorage).GetField(nameof(_items), BindingFlags.Instance | BindingFlags.NonPublic);
    async Task InitStorage()
    {
        if (Storage is null)
            Storage = new InteractableStorage();
        _items.SetValue(Storage, new Items(PlayerInventory.STORAGE));
        var size = GetSize(Player);
        StorageItems.resize(size.Width, size.Height);
        AddStorageEvents();
    }

    void RemoveStorageEvents()
    {
        if (Storage is null)
            return;
        StorageItems.onItemAdded -= ItemAdded;
        StorageItems.onItemRemoved -= ItemRemoved;
        StorageItems.onItemDiscarded -= ItemRemoved;
        StorageItems.onItemUpdated -= ItemUpdated;
    }
    void AddStorageEvents()
    {
        RemoveStorageEvents();
        if (Storage is null)
            return;
        StorageItems.onItemAdded += ItemAdded;
        StorageItems.onItemRemoved += ItemRemoved;
        StorageItems.onItemDiscarded += ItemRemoved;
        StorageItems.onItemUpdated += ItemUpdated;
    }

    public async Task Close()
    {
        await ReturnItem();
        await OpenStorage();
    }
    public async Task Show()
    {
        CloseStorage();
        SetActive(true);
        this[conf.Sell.SellToPlayers].UpdateVisibility(this, Model.Type.HasFlag(MarketItem.EType.Players));
        this[conf.Sell.SellToServer].UpdateVisibility(this, Model.Type.HasFlag(MarketItem.EType.Server));
        GetElement<Image>(conf.Sell.Image).UpdateImage(this, Model.IconUrl);
    }

    public void Hide()
    {
        Model = null;
        Item = null;
        SetActive(false);
    }

    MarketPlayerItem GetItem(PlayerData data, ItemJar jar) => data.SellingItems.FirstOrDefault(x => x.Id == jar.item.id && x.X == jar.x && jar.y == x.Y);

    MarketItem Model { get; set; }
    ItemJar Item { get; set; }

    Task RemoveItem()
    {
        if (Item is null)
            return Task.CompletedTask;
        StorageItems.items.Remove(Item);
        return OnItemRemoved(Item);
    }
    async Task ReturnItem()
    {
        if (Item is null)
            return;
        await RemoveItem();
        Inventory.forceAddItem(Item.item, true);
    }

    async void ItemUpdated(byte page, byte index, ItemJar jar)
    {
        using (var db = Main.DataBase)
        {
            var data = await db.GetOrAddAsync(Player.CSteamID);
            var item = GetItem(data, jar);
            if (item is null)
                return;
            item.Durability = jar.item.durability;
            item.State = jar.item.state;
            item.Amount = jar.item.amount;
        }
    }

    async void ItemAdded(byte page, byte index, ItemJar jar)
    {
        Item = jar;
        Model = conf.Items.FirstOrDefault(x => x.ID == jar.item.id);
        if (Model is null)
        {
            await ReturnItem();
            await Task.Delay(100);
            await OpenStorage();
            UnturnedChat.Say(Player, TranslateNotWhitelisted(jar.item.id));
        }
        else
            await Show();
    }

    async Task OnItemRemoved(ItemJar jar)
    {
        using (var db = Main.DataBase)
        {
            var data = await db.GetOrAddAsync(Player.CSteamID);
            data.SellingItems.Remove(GetItem(data, jar));
        }
    }
    async void ItemRemoved(byte page, byte index, ItemJar jar) => await OnItemRemoved(jar);
}