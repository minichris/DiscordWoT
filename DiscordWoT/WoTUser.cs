using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace DiscordWoT
{
    class WoTUser
    {
        public static string api = @"https://api.worldoftanks.eu/wot/";
        public static string WargamingKey = @"cece18ab7eb274e46c53ced7760b3e16";

        public ulong DiscordId;
        public string DiscordUsername;
        public ulong WoTID;
        public JObject WotPlayerData;

        public Task AddNewWoTUser(SocketUser DiscordUser, string GivenWoTName)
        {
            DiscordId = DiscordUser.Id;
            DiscordUsername = DiscordUser.Username;
            RetrieveWoTId(GivenWoTName);
            Console.WriteLine("Retrieving {0}'s personal data.", GivenWoTName);
            RetrieveWoTPersonalData();
            return Task.CompletedTask;
        }

        public Task RetrieveWoTId(string PlayerName)
        {
            string OptionsString = @"account/list/?application_id=" + WargamingKey + "&search=" + PlayerName + "&type=exact";
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString(api + OptionsString);
                dynamic JsonObject = JObject.Parse(json);
                WoTID = JsonObject.data[0].account_id;
                return Task.CompletedTask;
            }            
        }

        private Task RetrieveWoTPersonalData()
        {
            string OptionsString = @"account/info/?application_id=" + WargamingKey + "&account_id=" + WoTID;
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString(api + OptionsString);
                WotPlayerData = JObject.Parse(json);
                return Task.CompletedTask;
            }
        }
    }
}