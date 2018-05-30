using Discord;
using Discord.Commands;
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
        public JObject WoTPlayerVehiclesStats = new JObject();
        public JObject WoTPlayerVehiclesAchievements = new JObject();

        public WoTUser(ulong GivenDiscordId, ICommandContext ContextCaller = null) //Get a user from their discord ID provided they have an existing record on the server
        {
            if(File.Exists(FileLocation(GivenDiscordId)))
            {
                string Json = File.ReadAllText(FileLocation(GivenDiscordId));
                JsonConvert.PopulateObject(Json, this);

                //if the players data is older then a day, reretrieve it!
                Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                if (unixTimestamp - (Int32)WoTPlayerPersonalData["data"][WoTID.ToString()]["updated_at"] > 86400)
                {
                    Console.WriteLine(DiscordUsername + "'s player data is outdated, getting a fresh copy now...");
                    RetrieveWoTPlayerData((string)WoTPlayerPersonalData["data"][WoTID.ToString()]["nickname"]);
                }
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

        public WoTUser(IUser DiscordUser, string GivenWoTName)
        {
            DiscordId = DiscordUser.Id; //set this object's Discord User ID to the given one since we know its right
            DiscordUsername = DiscordUser.Username; //set this object's Discord Username to the given one since we know its right
            RetrieveWoTPlayerData(GivenWoTName);
        }

        private string FileLocation(ulong GivenDiscordId)
        {
            return "Users/" + GivenDiscordId.ToString() + ".json";
        }

        private string GetMasteryString(int MasteryBadge)
        {
            switch (MasteryBadge)
            {
                case 1:
                    return "3rd Class";
                case 2:
                    return "2nd Class";
                case 3:
                    return "1st Class";
                case 4:
                    return "Ace Tanker";
                default:
                    return "None";
            }
        }

        public List<string> TanksUserMastery(int Tier)
        {
            List<string> ReturnList = new List<string>();
            foreach (KeyValuePair<string, JToken> PlayerTank in WoTPlayerVehiclesStats)
            {
                try
                {
                    WoTTank WoTTankObj = new WoTTank(Int32.Parse(PlayerTank.Key));

                    //Check the tank still exists
                    if (WoTTankObj.WoTTankData == null)
                    {
                        continue;
                    }                    
                    
                    //if the tank is the same tier as the one we specified 
                    if (Convert.ToInt16((string)WoTTankObj.WoTTankData["tier"]) == Tier)
                    {
                        ReturnList.Add(WoTTankObj.WoTTankData["name"] + " mastery: " + GetMasteryString((int)PlayerTank.Value["mark_of_mastery"]));
                    }   
                }

                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("This error was found on tank number " + PlayerTank.Key);
                }
            }
            return ReturnList;
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

                //get the player vehicle stat data
                OptionsString = @"tanks/stats/?application_id=" + Program.WargammingKey + "&account_id=" + WoTID;
                json = new WebClient().DownloadString(api + OptionsString);
                JObject ReturnedStatJSON = JObject.Parse(json);
                if (ReturnedStatJSON["status"].ToString() == "error")
                {
                    throw new Exception("Error in returned PlayerVehicles for player " + PlayerName);
                }
                JToken WoTPlayerVehiclesStatTemp = ReturnedStatJSON["data"][WoTID.ToString()];
                foreach (JToken Vehicle in WoTPlayerVehiclesStatTemp)
                {
                    WoTPlayerVehiclesStats.Add(Vehicle["tank_id"].ToString(), Vehicle);
                }

                //get the player vehicle achievement data
                OptionsString = @"tanks/achievements/?application_id=" + Program.WargammingKey + "&account_id=" + WoTID;
                json = new WebClient().DownloadString(api + OptionsString);
                JObject ReturnedAchievementJSON = JObject.Parse(json);
                if (ReturnedAchievementJSON["status"].ToString() == "error")
                {
                    throw new Exception("Error in returned PlayerVehicles for player " + PlayerName);
                }
                JToken WoTPlayerVehiclesAchivementTemp = ReturnedAchievementJSON["data"][WoTID.ToString()];
                foreach (JToken Vehicle in WoTPlayerVehiclesAchivementTemp)
                {
                    WoTPlayerVehiclesAchievements.Add(Vehicle["tank_id"].ToString(), Vehicle);
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