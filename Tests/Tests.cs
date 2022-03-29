using System.Threading.Tasks;
using Steamworks;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using SMarket.Models;
using System.Linq.Expressions;
using SMarket.Views;

namespace Tests
{
    public class Tests
    {
        [Test]
        public async Task DBTest()
        {
            var db = new SMarket.DataBase.MySqlDataBase();
            await db.Database.EnsureDeletedAsync();
            await db.Database.EnsureCreatedAsync();
            var data = await db.GetOrAddAsync(CSteamID.Nil);
            data.SellingItems.Add(new() { Price = 5 });
            db.Dispose();
            db = new SMarket.DataBase.MySqlDataBase();
            data = await db.GetOrAddAsync(CSteamID.Nil);
            Console.WriteLine(data.SellingItems.First().Price);
            db.Dispose();
            Assert.Pass();
        }
    }
}