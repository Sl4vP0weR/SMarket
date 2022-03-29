using System.Linq.Expressions;
using Cysharp.Threading.Tasks;
using SMarket.Controllers;
using SMarket.DataBase;
using UnityEngine.UI;

namespace SMarket
{
    public sealed class Main : RocketPlugin<Config>
    {
        /// <summary>
        /// Creates and returns new context of DataBase.
        /// </summary>
        public static IDataBase DataBase => DataBaseFactory!.Invoke();
        public static Func<IDataBase> DataBaseFactory;
        public static Main Instance { get; private set; }

        protected override void Unload()
        {
            if (Instance is null)
                return;
            StopAllCoroutines();
            Provider.clients.ForEach(x => PlayerUpdateLastName(UP.FromSteamPlayer(x)));
            U.Events.OnPlayerConnected -= PlayerUpdateLastName;
            U.Events.OnPlayerDisconnected -= PlayerUpdateLastName;
            Instance = null;
        }
        protected override async void Load()
        {
            Instance = this;
            DataBaseFactory = () => new MySqlDataBase(conf.DBSettings);
            using (var db = DataBase) { }
            U.Events.OnPlayerConnected += PlayerUpdateLastName;
            U.Events.OnPlayerDisconnected += PlayerUpdateLastName;
        }

        public async void PlayerUpdateLastName(UP up)
        {
            using (var db = DataBase)
            {
                var data = await db.GetOrAddAsync(up.CSteamID);
                data.LastName = up.CharacterName;
            }
        }

        #region Translations
        public override TranslationList DefaultTranslations => DefaultTranslationList;

        public new string Translate(string key, params object[] args) => base.Translate(key.Trim(TranslationKeyTrimCharacters), args);
        #endregion
    }
}