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
        public JObject WoTPlayerPersonalData;
        public JObject WoTPlayerVehicles;

        public WoTUser(ulong GivenDiscordId) //Get a user from their discord ID provided they have an existing record on the server
        {
            if(File.Exists(FileLocation(GivenDiscordId)))
            {
                string Json = File.ReadAllText(FileLocation(GivenDiscordId));
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

        private string FileLocation(ulong GivenDiscordId)
        {
            return "Users/" + GivenDiscordId.ToString() + ".json";
        }

        private Task RetrieveWoTPlayerData(string PlayerName)
        {
            Console.WriteLine("Retrieving player data for " + PlayerName + " from WoT servers.");
            try
            {
                //search for the player by ID
                string OptionsString = @"account/list/?application_id=" + Program.WargammingKey + "&search=" + PlayerName + "&type=exact";
                string json = new WebClient().DownloadString(api + OptionsString);
                dynamic JsonObject = JObject.Parse(json);
                WoTID = JsonObject.data[0].account_id;
                if (JsonObject["status"].ToString() == "error")
                {
                    throw new Exception("Error in returned search for player " + PlayerName);
                }

                //Get the player personal data
                OptionsString = @"account/info/?application_id=" + Program.WargammingKey + "&account_id=" + WoTID;
                json = new WebClient().DownloadString(api + OptionsString);
                WoTPlayerPersonalData = JObject.Parse(json);
                if(WoTPlayerPersonalData["status"].ToString() == "error")
                {
                    throw new Exception("Error in returned PersonalData for player " + PlayerName);
                }

                //get the player vehicle data
                OptionsString = @"account/tanks/?application_id=" + Program.WargammingKey + "&account_id=" + WoTID;
                json = new WebClient().DownloadString(api + OptionsString);
                WoTPlayerVehicles = JObject.Parse(json);
                if (WoTPlayerVehicles["status"].ToString() == "error")
                {
                    throw new Exception("Error in returned PlayerVehicles for player " + PlayerName);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //write the player data to a file
            using (StreamWriter writer = File.CreateText(FileLocation(DiscordId)))
            {
                writer.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
            Console.WriteLine("Retrieving player data for " + PlayerName + " completed.");
            return Task.CompletedTask;
        }
    }
}