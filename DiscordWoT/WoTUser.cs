using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;

namespace DiscordWoT
{
    class WoTUser
    {
        public static string api = @"https://api.worldoftanks.eu/wot/";
        public static string WargamingKey = @"cece18ab7eb274e46c53ced7760b3e16";

        public ulong DiscordId;
        public string DiscordUsername;
        public ulong WoTID;

        public bool AddNewWoTUser(SocketUser DiscordUser, string GivenWoTName)
        {
            DiscordId = DiscordUser.Id;
            DiscordUsername = DiscordUser.Username;
            WoTID = GetWoTAccountID(GivenWoTName);
            Console.WriteLine(WoTID);
            return true;
        }

        public ulong GetWoTAccountID(string PlayerName)
        {
            string OptionsString = @"account/list/?application_id=" + WargamingKey + "&search=" + PlayerName + "&type=exact";
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString(api + OptionsString);
                dynamic JsonObject = JObject.Parse(json);
                return JsonObject.data.account_id;
            }            
        }
    }
}