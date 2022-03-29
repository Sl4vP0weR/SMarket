using System.Threading.Tasks;
using SMarket.Entities;
using SMarket.Views;

namespace SMarket.Controllers;

public partial class MarketController
{
    public class PlayersTabContext : TabContext
    {
        public PlayersTabContext(MarketController controller) : base(controller) { }

        public override async Task<IEnumerable<MarketItemView>> GetDataSource()
        {
            using var db = Main.DataBase;
            return await db.GetAllItems();
        }

        protected override async Task<bool> ItemExists(MarketItemView item)
        {
            if (items is null)
                return false;
            if (item is MarketPlayerItem playerItem)
            {
                using var db = Main.DataBase;
                return await db.GetItemAsync(playerItem.Identifier) is not null;
            }
            return false;
        }
        protected override async Task RemoveItem(MarketItemView item)
        {
            items.Remove(item);
            if (item is MarketPlayerItem playerItem)
                try
                {
                    using var db = Main.DataBase;
                    await db.RemoveItemAsync(playerItem.Identifier);
                }
                catch { }
            await Render();
        }
    }
}