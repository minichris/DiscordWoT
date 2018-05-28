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
    class WoTTank
    {
        public static string api = @"https://api.worldoftanks.eu/wot/";

        public JObject WoTTankData;

        public WoTTank(int tankID) //Get a user from their discord ID provided they have an existing record on the server
        {
            if(File.Exists(FileLocation(tankID)))
            {
                string Json = File.ReadAllText(FileLocation(tankID));
                JsonConvert.PopulateObject(Json, this);
            }
            else
            {
                RetrieveWoTTankData(tankID);
            }
        }

        private string FileLocation(int GivenTankId)
        {
            return "Tanks/" + GivenTankId.ToString() + ".json";
        }

        private Task RetrieveWoTTankData(int WoTTankID)
        {
            Console.WriteLine("Retrieving tank data for " + WoTTankID + " from WoT servers.");
            try
            {
                string OptionsString = @"encyclopedia/vehicles/?application_id=" + Program.WargammingKey + "&tank_id=" + WoTTankID;
                string json = new WebClient().DownloadString(api + OptionsString);
                JObject ReturnedJSONData = JObject.Parse(json);
                if (ReturnedJSONData["status"].ToString() == "error")
                {
                    throw new Exception("Error in returned for tank " + WoTTankID);
                }
                else
                {
                    WoTTankData = ReturnedJSONData["data"][WoTTankID.ToString()].ToObject<JObject>();
                }

                //write the tank data to a file
                using (StreamWriter writer = File.CreateText(FileLocation(WoTTankID)))
                {
                    writer.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
                }     
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("Retrieving tank data for " + WoTTankID + " completed.");
            return Task.CompletedTask;
        }
    }
}