using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DiscordWoT
{
    partial class Program
    {
        private async Task CommandReceived(SocketMessage message)
        {
            if (message.Content.ToLower() == "~ping")
            {
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync("Pong~");
            }
            else if (message.Content.ToLower().StartsWith("~add "))
            {
                string PossibleWoTName = message.Content.ToLower().Replace("~add ", "");
                try
                {
                    WoTUser UserObj = new WoTUser(message.Author, PossibleWoTName);
                    await message.Channel.SendMessageAsync("Done!");
                }
                catch(ArgumentOutOfRangeException)
                {
                    await message.Channel.SendMessageAsync($"Failed! Seems like \"{PossibleWoTName}\" doesn't exist :(");
                }
                catch(Exception e)
                {
                    await message.Channel.SendMessageAsync(e.Message);
                }
                await message.DeleteAsync();
            }
            else if (message.Content.ToLower().StartsWith("~me"))
            {
                await WotGetMeMessage(message);
                await message.DeleteAsync();
            }
            else if (message.Content.ToLower().StartsWith("~sig"))
            {
                await WoTGetSignature(message);
                await message.DeleteAsync();
            }
            else if (message.Content.ToLower().StartsWith("~help"))
            {
                EmbedBuilder EmbedObj = new EmbedBuilder();
                EmbedObj.WithTitle("Help");
                EmbedObj.AddField("~help", "Show this message.");
                EmbedObj.AddField("~add <WoT Username>", "Add your WoT username.");
                EmbedObj.AddField("~me", "Show some of your player statistics. Can only be done after adding your WoT username.");
                EmbedObj.AddField("~sig", "Show your stats signature. Can only be done after adding your WoT username.");
                EmbedObj.WithDescription("All of my current commands.");
                EmbedObj.WithThumbnailUrl("http://i0.kym-cdn.com/photos/images/original/001/170/314/f3d.png");
                EmbedObj.WithColor(Color.Magenta);
                EmbedObj.WithFooter("Version " + VersionString, "https://vignette.wikia.nocookie.net/gup/images/d/d7/GUP_OoaraiSmall_9335.png");
                await message.Channel.SendMessageAsync("", false, EmbedObj);
                await message.DeleteAsync();
            }
        }

        private Task WotGetMeMessage(SocketMessage InitiatorMessage)
        {
            try
            {
                WoTUser WoTUserObj = new WoTUser(InitiatorMessage.Author.Id);
                var PlayersData = WoTUserObj.WoTPlayerPersonalData["data"][WoTUserObj.WoTID.ToString()];
                string PlayerTreesCut = PlayersData["statistics"]["trees_cut"].ToString();
                string PlayerNickname = PlayersData["nickname"].ToString();
                InitiatorMessage.Channel.SendMessageAsync("Hello " + PlayerNickname + ", you have cut down " + PlayerTreesCut + " trees!");
                return Task.CompletedTask;
            }
            catch (ArgumentException)
            {
                InitiatorMessage.Channel.SendMessageAsync("You need to register yourself with the ~add command. See ~help for usage details.");
                return Task.CompletedTask;
            }
        }

        private async Task WoTGetSignature(SocketMessage InitiatorMessage)
        {
            try
            {
                WoTUser WoTUserObj = new WoTUser(InitiatorMessage.Author.Id);
                string Filename = "Users/" + WoTUserObj.WoTID + ".png";
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile("http://wotlabs.net/sig_cust/FFFFFF/36393E/eu/" + WoTUserObj.WoTPlayerPersonalData["data"][WoTUserObj.WoTID.ToString()]["nickname"].ToString() + "/signature.png", Filename);
                    await InitiatorMessage.Channel.SendFileAsync(Filename);
                    File.Delete(Filename);
                }
            }
            catch (ArgumentException)
            {
                await InitiatorMessage.Channel.SendMessageAsync("You need to register yourself with the ~add command. See ~help for usage details.");
            }
        }
    }
}
