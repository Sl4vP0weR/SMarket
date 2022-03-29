namespace SMarket.Models;

public class MarketItem : MarketItemBase
{
    [XmlAttribute]
    public ushort ID { get; set; }
    [XmlAttribute]
    public EType Type { get; set; } = EType.Both;
    [XmlAttribute]
    public decimal ServerCost, MinCost, MaxCost;
    public bool Validate(EType type, decimal cost) => (Type == type || Type.HasFlag(type)) && type switch
    {
        EType.Players => cost >= MinCost && cost <= MaxCost,
        _ => true
    };

    [Flags]
    public enum EType
    {
        Server = 1,
        Players = 2,
        Both = Server | Players
    }
}