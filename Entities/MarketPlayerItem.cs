using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SMarket.Controllers;
using SMarket.Models;
using SMarket.Views;

namespace SMarket.Entities;

public class MarketPlayerItem : MarketItemView
{
    public MarketPlayerItem() { }
    public MarketPlayerItem(ItemJar jar)
    {
        Id = jar.item.id;
        State = jar.item.state;
        Amount = jar.item.amount;
        Durability = jar.item.durability;
        X = jar.x;
        Y = jar.y;
        SizeX = jar.size_x;
        SizeY = jar.size_y;
        Rotation = jar.rot;
    }
    [XmlIgnore, Key]
    public Guid Identifier { get; set; }
    [XmlIgnore]
    public string OwnerId { get; set; }
    [XmlIgnore, ForeignKey(nameof(OwnerId))]
    public virtual PlayerData Owner { get; set; }
    public MarketItem Base => conf.Items.FirstOrDefault(x => x.ID == Id);
    public ItemAsset Asset => Assets.find(EAssetType.ITEM, Id.Value) as ItemAsset;
    public override string IconUrl => Base?.IconUrl;
    public override string Name => Base?.FormatName(Asset);
    public override string Description => Base?.FormatDescription(Asset);
    public override string Seller => Owner?.LastName;
    public ushort? Id { get; set; }
    public byte[]? State { get; set; }
    public byte? Durability { get; set; }
    public byte? X { get; set; }
    public byte? Y { get; set; }
    public byte? SizeX { get; set; }
    public byte? SizeY { get; set; }
    public byte? Rotation { get; set; }
    public Item GetItem() => new(Id.Value, Amount.Value, Durability.Value, State);
    public ItemJar GetJar() => new(X.Value, Y.Value, Rotation.Value, GetItem()) { size_x = SizeX.Value, size_y = SizeY.Value };
    public override void OnPurchased(UnturnedPlayer buyer)
    {
        var owner = PlayerTool.getPlayer(Owner.CSteamID);
        if (owner)
            owner.GetComponent<SellController>().CloseStorage();
        buyer.Inventory.forceAddItem(GetItem(), true);
        uconomy.IncreaseBalance(Owner.Id.ToString(), Price);
    }
}