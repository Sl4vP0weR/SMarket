using System.Threading.Tasks;
using SMarket.Entities;
using SMarket.Models;
using SMarket.Views;

namespace SMarket.Controllers;

public partial class MarketController
{
    public abstract class TabContext
    {
        public MarketController Controller { get; }
        public UP Player => Controller.Player;
        public short Key => Controller.Key;
        public TabContext(MarketController controller)
        {
            Controller = controller;
        }
        public abstract Task<IEnumerable<MarketItemView>> GetDataSource();
        protected List<MarketItemView> items;
        public IReadOnlyList<MarketItemView> Items => items;
        public int PagesCount
        {
            get
            {
                float count = Items?.Count ?? 0;
                var divided = Mathf.FloorToInt(count / PageSize);
                if (count % PageSize != 0)
                    ++divided;
                return divided;
            }
        }
        public int PageSize => conf.MarketPageSize;
        public int CurrentPageIndex { get; protected set; }
        public void SetSource(List<MarketItemView> newSource)
        {
            items = newSource;
            CurrentPageIndex = 0;
        }
        public async Task ResetItems()
        {
            items = (await GetDataSource()).ToList();
        }
        public async Task Search(string filter)
        {
            await ResetItems();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                filter = filter.ToLower();
                SetSource(items.Where(x => x.Name.ToLower().Contains(filter)).OrderBy(x => x.Name).ToList());
            }
            await Render();
        }
        public IReadOnlyList<MarketItemView> GetPageItems(int? page = null)
        {
            var list = new List<MarketItemView>();
            var idx = GetPageIndex(page);
            for (int i = 0; i < items.Count && i < PageSize; i++)
                list.Add(items.ElementAtOrDefault(idx + i));
            return list;
        }
        protected int GetPageIndex(int? page = null) => (page ?? CurrentPageIndex) * PageSize;

        public async Task Buy(int id)
        {
            id -= conf.Market.Row.StartIndex;
            var item = items.ElementAtOrDefault(GetPageIndex() + id);
            if (!await ItemExists(item))
            {
                UnturnedChat.Say(Player, TranslateSomethingWrong(), true);
                goto remove;
            }
            var sid = Player.Id;
            if (uconomy.GetBalance(sid) < item.Price)
                return;
            uconomy.IncreaseBalance(sid, -item.Price);
            try
            {
                item.OnPurchased(Player);
            }
            catch 
            {
                uconomy.IncreaseBalance(sid, item.Price);
                return;
            }
            try
            {
                UnturnedChat.Say(Player, TranslatePurchased(item.Name, Math.Round(item.Price, 2)), true);
            } catch { }
            remove:
            await RemoveItem(item);
        }
        protected virtual Task RemoveItem(MarketItemView item) => Task.FromResult(items.Remove(item));
        protected virtual Task<bool> ItemExists(MarketItemView item) => Task.FromResult(items.Contains(item));

        public Task OpenNextPage()
        {
            if (CurrentPageIndex + 1 < PagesCount)
                CurrentPageIndex++;
            else return Task.CompletedTask;

            return Render();
        }

        public Task OpenPreviousPage()
        {
            if (CurrentPageIndex > 0)
                CurrentPageIndex--;
            else return Task.CompletedTask;

            return Render();
        }

        public async Task Render()
        {
            if (items is null)
                await ResetItems();
            var tasks = new List<Task>();
            try
            {
                var page = GetPageItems();
                for (int i = 0; i < PageSize; i++)
                    tasks.Add(RenderItem(page.ElementAtOrDefault(i), i));
            }
            catch { }

            tasks.Add(ShowLoading());
            await Task.WhenAll(tasks);
        }

        CancellationTokenSource loadingCancellation;
        public async Task ShowLoading()
        {
            loadingCancellation?.Cancel();
            loadingCancellation = new CancellationTokenSource();
            var token = loadingCancellation.Token;
            try
            {
                EffectManager.sendUIEffectVisibility(Key, Player.CSteamID, true, conf.Market.Loading, true);
                await Task.Delay(750, token);
                EffectManager.sendUIEffectVisibility(Key, Player.CSteamID, true, conf.Market.Loading, false);
            }
            catch { }
        }

        protected virtual async Task RenderItem(MarketItemView item, int index)
        {
            var empty = item is null;
            var row = conf.Market.Row;
            var prefix = row.Prefix + (index+row.StartIndex);
            var key = Key;
            var id = Player.CSteamID;
            EffectManager.sendUIEffectVisibility(key, id, true, prefix + row.NamePostfix, !empty);
            if (empty)
                return;
            EffectManager.sendUIEffectImageURL(key, id, true, prefix + row.ImagePostfix, item.IconUrl, shouldCache: true);
            EffectManager.sendUIEffectText(key, id, true, prefix + row.ItemPostfix, item.Name);
            EffectManager.sendUIEffectText(key, id, true, prefix + row.DescriptionPostfix, item.Description);
            EffectManager.sendUIEffectText(key, id, true, prefix + row.PricePostfix, TranslatePrice(Math.Round(item.Price, 2)));
            EffectManager.sendUIEffectText(key, id, true, prefix + row.AmountPostfix, TranslateAmount(item.Amount ?? 1));
            EffectManager.sendUIEffectText(key, id, true, prefix + row.SellerPostfix, item.Seller);
        }
    }
}