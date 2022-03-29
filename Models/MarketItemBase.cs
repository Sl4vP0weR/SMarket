using System.ComponentModel.DataAnnotations.Schema;
using SMarket.Entities;

namespace SMarket.Models;

[XmlType("Item")]
public class MarketItemBase
{
    [XmlAttribute, NotMapped]
    public virtual string Name { get; set; }
    [XmlAttribute, NotMapped]
    public virtual string Description { get; set; }
    [XmlAttribute, NotMapped]
    public virtual string IconUrl { get; set; }
    public string FormatName(ItemAsset asset = null) => string.Format(Name, asset?.itemName);
    public string FormatDescription(ItemAsset asset = null) => string.Format(Name, asset?.itemDescription);
}
