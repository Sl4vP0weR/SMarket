using System.Linq.Expressions;
using System.Threading.Tasks;
using SMarket.Entities;

namespace SMarket.DataBase;

public interface IDataBase : IDisposable
{
    Task<PlayerData> GetOrAddAsync(CSteamID steamId);
    Task<IEnumerable<MarketPlayerItem>> GetAllItems();
    Task<MarketPlayerItem> GetItemAsync(Guid id);
    Task RemoveItemAsync(Guid id);
    Task SaveChangesAsync();
    Task<bool> TryConnectAsync();
}
