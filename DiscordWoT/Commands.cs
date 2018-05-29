using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

namespace DiscordWoT
{
    public class Info : ModuleBase
    {
        // ~say hello -> hello
        [Command("say"), Summary("Echos a message.")]
        public async Task Say([Remainder, Summary("The text to echo")] string echo)
        {
            if(Context.Message.Author.Id == 81333996977401856)
                await ReplyAsync(echo);
        }

        // ~add
        [Command("add"), Summary("Adds a players WoT.")]
        public async Task Add([Remainder, Summary("The player's WoT name.")] string Nickname)
        {
            try
            {
                WoTUser UserObj = new WoTUser(Context.Message.Author, Nickname);
                await Context.Message.Channel.SendMessageAsync("Done!");
            }
            catch (ArgumentOutOfRangeException)
            {
                await Context.Message.Channel.SendMessageAsync($"Failed! Seems like \"{Nickname}\" doesn't exist :(");
            }
            catch (Exception e)
            {
                await Context.Message.Channel.SendMessageAsync(e.Message);
            }
        }

        // ~ping -> pong
        [Command("ping"), Summary(@"Replies with 'Pong!'.")]
        public async Task Ping()
        {
            await ReplyAsync("Pong!");
        }

        // ~sig
        [Command("sig"), Summary(@"Get the player's signature.")]
        public async Task Sig()
        {
            try
            {
                WoTUser WoTUserObj = new WoTUser(Context.Message.Author.Id);
                string Filename = "Users/" + WoTUserObj.WoTID + ".png";
                using (WebClient wc = new WebClient())
                {
                    wc.DownloadFile("http://wotlabs.net/sig_cust/FFFFFF/36393E/eu/" + WoTUserObj.WoTPlayerPersonalData["data"][WoTUserObj.WoTID.ToString()]["nickname"].ToString() + "/signature.png", Filename);
                    await Context.Message.Channel.SendFileAsync(Filename);
                    File.Delete(Filename);
                }
            }
            catch (ArgumentException)
            {
                await Context.Message.Channel.SendMessageAsync("You need to register yourself with the ~add command. See ~help for usage details.");
            }
        }

        // ~me
        [Command("me"), Summary("Shows how many trees the player has cut down.")]
        public async Task Me()
        {
            try
            {
                WoTUser WoTUserObj = new WoTUser(Context.Message.Author.Id);
                var PlayersData = WoTUserObj.WoTPlayerPersonalData["data"][WoTUserObj.WoTID.ToString()];
                string PlayerTreesCut = PlayersData["statistics"]["trees_cut"].ToString();
                string PlayerNickname = PlayersData["nickname"].ToString();
                Context.Message.Channel.SendMessageAsync("Hello " + PlayerNickname + ", you have cut down " + PlayerTreesCut + " trees!");
            }
            catch (ArgumentException)
            {
                Context.Message.Channel.SendMessageAsync("You need to register yourself with the ~add command. See ~help for usage details.");
            }
        }

        // ~help
        [Command("help"), Summary(@"Shows the help dialogue.")]
        public async Task Help()
        {
            EmbedBuilder EmbedObj = new EmbedBuilder();
            EmbedObj.WithTitle("Help");
            EmbedObj.AddField("~help", "Show this message.");
            EmbedObj.AddField("~add <WoT Username>", "Add your WoT username.");
            EmbedObj.AddField("~me", "Show some of your player statistics. Can only be done after adding your WoT username.");
            EmbedObj.AddField("~sig", "Show your stats signature. Can only be done after adding your WoT username.");
            EmbedObj.AddField("~tanks <tier>", "Show your tank mastery. Can only be done after adding your WoT username.");
            EmbedObj.WithDescription("All of my current commands.");
            EmbedObj.WithThumbnailUrl("http://i0.kym-cdn.com/photos/images/original/001/170/314/f3d.png");
            EmbedObj.WithColor(Color.Magenta);
            EmbedObj.WithFooter("Version " + Program.VersionString, "https://vignette.wikia.nocookie.net/gup/images/d/d7/GUP_OoaraiSmall_9335.png");
            await Context.Message.Channel.SendMessageAsync("", false, EmbedObj);
        }

        // ~tanks
        [Command("tanks"), Summary("Shows player tank masteries.")]
        public async Task Tanks([Remainder, Summary("The text to echo")] string tier)
        {
            if (Int32.TryParse(tier, out int RequestedTier))
            {
                WoTUser WoTUserObj = new WoTUser(Context.Message.Author.Id);
                await Context.Message.Channel.SendMessageAsync(WoTUserObj.WoTPlayerPersonalData["data"][WoTUserObj.WoTID.ToString()]["nickname"] + "'s tier " + RequestedTier + " tank mastery:");
                string outputmessage = string.Join("\n", WoTUserObj.TanksUserMastery(RequestedTier));
                await Context.Message.Channel.SendMessageAsync(outputmessage);
            }
            else
            {
                Context.Message.Channel.SendMessageAsync("Command usage: ~tanks <tier>");
            }
        }
    }

    partial class Program
    {
        public async Task HandleCommand(SocketMessage messageParam)
        {
            var message = messageParam as SocketUserMessage;
            if (message == null) return;
            int argPos = 0;
            if (!(message.HasCharPrefix('~', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos))) return;
            var context = new CommandContext(_client, message);
            message.DeleteAsync();
            var result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
        }
    }
}
