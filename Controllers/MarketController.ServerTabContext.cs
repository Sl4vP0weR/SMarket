using System.Threading.Tasks;
using SMarket.Views;

namespace SMarket.Controllers;

public partial class MarketController
{
    public class ServerTabContext : TabContext
    {
        public ServerTabContext(MarketController controller) : base(controller) { }

        public override async Task<IEnumerable<MarketItemView>> GetDataSource() => conf.ServerItems;

        protected override Task<bool> ItemExists(MarketItemView item)
        {
            var False = Task.FromResult(false);
            if (items is null)
                return False;
            if (item is null)
                return False;
            return Task.FromResult(conf.ServerItems.Contains(item));
        }
        protected override Task RemoveItem(MarketItemView item)
        {
            if (item is not null)
                return Task.CompletedTask;
            items.Remove(item);
            return Render();
        }
    }
}