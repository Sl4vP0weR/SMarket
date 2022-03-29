using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SMarket.Models;

namespace SMarket.Views;

public class MarketItemView : MarketItemBase
{
    [XmlAttribute]
    public virtual decimal Price { get; set; }
    [XmlIgnore]
    public virtual byte? Amount { get; set; }
    [XmlIgnore]
    public virtual string Seller { get; }

    public virtual void OnPurchased(UP buyer) { }
}
