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

        public ulong DiscordId;
        public string DiscordUsername;
        public ulong WoTID;
        public JObject WotPlayerData;

        public WoTUser(ulong GivenDiscordId) //Get a user from their discord ID provided they have an existing record on the server
        {
            if(File.Exists("Users/" + GivenDiscordId))
            {
                string Json = File.ReadAllText("Users/" + GivenDiscordId);
                JsonConvert.PopulateObject(Json, this);
            }
            else
            {
                throw new ArgumentException("User lacks an existing record.");
            }
        }

        public WoTUser(SocketUser DiscordUser, string GivenWoTName)
        {
            DiscordId = DiscordUser.Id; //set this object's Discord User ID to the given one since we know its right
            DiscordUsername = DiscordUser.Username; //set this object's Discord Username to the given one since we know its right
            RetrieveWoTPlayerData(GivenWoTName);
        }

        private Task RetrieveWoTPlayerData(string PlayerName)
        {
            string OptionsString = @"account/list/?application_id=" + Program.WargammingKey + "&search=" + PlayerName + "&type=exact";
            string json = new WebClient().DownloadString(api + OptionsString);
            dynamic JsonObject = JObject.Parse(json);
            WoTID = JsonObject.data[0].account_id;
            OptionsString = @"account/info/?application_id=" + Program.WargammingKey + "&account_id=" + WoTID;
            json = new WebClient().DownloadString(api + OptionsString);
            WotPlayerData = JObject.Parse(json);
            using (StreamWriter writer = File.CreateText("Users/" + DiscordId.ToString()))
            {
                writer.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            return Task.CompletedTask;
        }
    }
}