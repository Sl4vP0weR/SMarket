using System.ComponentModel.DataAnnotations;

namespace SMarket.Entities;

public class PlayerData
{
    public PlayerData() { }
    public PlayerData(CSteamID id)
    {
        Id = id.ToString();
    }
    public CSteamID CSteamID => new(ulong.Parse(Id));
    public string? LastName { get; set; }
    [Key]
    public string Id { get; set; }
    public virtual List<MarketPlayerItem> SellingItems { get; set; } = new List<MarketPlayerItem>();
}
