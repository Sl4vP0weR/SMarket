using MySql.Data.MySqlClient;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using SMarket.Entities;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
using System.Net;
using System.Runtime.InteropServices;

namespace SMarket.DataBase;

public class MySqlDataBase : DbContext, IDataBase, IDisposable
{
    static IServiceProvider ServiceProvider;
    public static IMemoryCache MemoryCache => ServiceProvider.GetService<IMemoryCache>();
    static void RegisterServices()
    {
        var services = new ServiceCollection();
        services
            .AddMemoryCache();
        ServiceProvider = services.BuildServiceProvider();
    }
    static MySqlDataBase()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        RegisterServices();
    }
    public static MySqlDataBase Create(DataBaseSettings options = null)
    {
        try
        {
            var context = new MySqlDataBase(options ?? new());
            return context;
        }
        catch (Exception ex) { throw ex; }
    }
    /// <summary>
    /// Strongly unrecommended
    /// </summary>
    public MySqlDataBase() : this(new()) { }
    public MySqlDataBase(DataBaseSettings options)
    {
        if (ServiceProvider is null)
            RegisterServices();

        Settings = options;

        Database.EnsureCreated();
        try
        {
            try
            {
                Database.Migrate();
            }
            catch { }
            var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            databaseCreator.CreateTables();
        }
        catch { }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        base.OnConfiguring(options);
        options.UseMemoryCache(null);
        var builder = new MySqlConnectionStringBuilder(Settings.ConnectionString);
        if (System.Environment.OSVersion.Platform == PlatformID.Unix)
        {
            builder.SslMode = MySqlSslMode.None;
        }
        options.UseMySQL(builder.ToString());
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        var item = model
            .Entity<MarketPlayerItem>()
            .ToTable($"{Settings.Table}_items");

        item
            .HasKey(x => x.Identifier);

        var player = model
             .Entity<PlayerData>()
             .ToTable(Settings.Table);

        player
            .HasKey(x => x.Id);
        player
            .Property(x => x.Id)
            .ValueGeneratedNever();
        player
            .HasIndex(x => x.Id)
            .IsUnique();
    }

    public DataBaseSettings Settings { get; }

    public DbSet<PlayerData> Players { get; set; }
    public DbSet<MarketPlayerItem> Items { get; set; }

    public void Update(PlayerData data) => Players.Update(data);
    public async Task<PlayerData> GetOrAddAsync(CSteamID steamId)
    {
        var id = steamId.ToString();
        var result = await Players
                .Include(x => x.SellingItems)
                .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
        {
            result = new(steamId);
            await Players.AddAsync(result);
            await SaveChangesAsync();
        }
        return result;
    }

    public async Task<bool> TryConnectAsync()
    {
        try
        {
            var token = new CancellationTokenSource(TimeSpan.FromSeconds(3)).Token;
            var connected = await Database.CanConnectAsync(token);
            return connected;
        }
        catch (MySqlException ex)
        {
            try
            {
                Logger.Log(ex);
            }
            catch { Console.WriteLine(ex); }
        }
        catch (Exception) { }
        return false;
    }

    public async Task<IEnumerable<MarketPlayerItem>> GetAllItems() => await Items.Include(x => x.Owner).AsNoTracking().ToListAsync();
    public Task<MarketPlayerItem> GetItemAsync(Guid id) => Items.Include(x => x.Owner).FirstOrDefaultAsync(x => x.Identifier == id);
    public async Task RemoveItemAsync(Guid id)
    {
        var item = await GetItemAsync(id);
        if (item is null)
            return;
        Items.Remove(item);
        await SaveChangesAsync();
    }

    Task IDataBase.SaveChangesAsync() => SaveChangesAsync();

    void IDisposable.Dispose()
    {
        try
        {
            SaveChanges();
        }
        catch (Exception ex) { Logger.Log(ex); }
        base.Dispose();
    }
}