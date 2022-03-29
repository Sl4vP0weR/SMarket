using SMarket.DataBase;
using SMarket.Models;

namespace SMarket
{
    public class ControllerUIPreset
    {
        [XmlAttribute]
        public ushort ID = 1000;
        [XmlAttribute]
        public short Key = -1000;
    }
    public class MarketRow
    {
        public int StartIndex = 1;
        public string
            Prefix = "Market_Menu_Table_Row_",
            NamePostfix = "_Info",
            ImagePostfix = "_Image",
            ItemPostfix = "_Item",
            DescriptionPostfix = "_Description",
            PricePostfix = "_Price",
            SellerPostfix = "_Seller",
            AmountPostfix = "_Amount",
            Buy = "Market_Menu_Table_Row_{0}_Button_Sell";
    }
    public class MarketTabs
    {
        public string
            Players = "Market_Button_Flea",
            Server = "Market_Button_Black";
    }
    public class MarketSearch
    {
        public string
            Placeholder = "Market_Menu_Search_Placeholder",
            Text = "Market_Menu_Search_Text",
            Button = "Market_Menu_Button_Search";
    }
    public class MarketPagination
    {
        public string
            Page = "Market_Menu_Page",
            Previous = "Market_Menu_Button_Previous",
            Next = "Market_Menu_Button_Next";
    }
    public class MarketUI : ControllerUIPreset
    {
        public MarketTabs Tabs = new();
        public MarketRow Row = new();
        public MarketSearch Search = new();
        public MarketPagination Pagination = new();
        public string 
            Close = "Market_Menu_Button_Close",
            Menu = "Market_Menu",
            Loading = "Market_Menu_Table_Loading",
            Total = "Market_Menu_Count";
    }
    public class SellUI : ControllerUIPreset
    {
        public string 
            Title = "Market_SellCard_Title",
            Cost = "Market_SellCard_Input",
            Image = "Market_SellCard_Image_Holder",
            SellToPlayers = "Market_SellCard_Button_PutUp",
            SellToServer = "Market_SellCard_Button_Sell",
            Close = "Market_SellCard_Close";
    }
    public record struct Size([XmlAttribute] byte Width = 5, [XmlAttribute] byte Height = 5);
    public partial class Config : IRocketPluginConfiguration
    {
        public DataBaseSettings DBSettings = new ();
        public int MarketPageSize = 13;
        public string PlayersItemsTabName, ServerItemsTabName;
        public MarketUI Market = new ();
        public SellUI Sell = new();
        public string SizePermission = "market.size.{x}.{y}";
        public Size DefaultSize;
        public List<MarketItem> Items = new();
        public List<MarketServerItem> ServerItems = new();
        public void LoadDefaults()
        {
            ServerItems.Add(new() { Commands = new() { "give {0}/519" }, Description = "Rocket Launcher", Name = "RPG", Price = 100 });
            Items.Add(new() { Name = "{0}", ID = 519, IconUrl = "unticons.com/i_519.png", ServerCost = 100, MinCost = 50, MaxCost = 1000 });
        }
    }
}