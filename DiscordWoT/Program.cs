using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace DiscordWoT
{
    class Program
    {
        static string VersionString = "0.1.2";

        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private DiscordSocketClient _client;

        public async Task MainAsync()
        {
            Directory.CreateDirectory("Users");
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
                WebSocketProvider = WS4NetProvider.Instance
            });

            _client.Log += Log;
            _client.MessageReceived += MessageReceived;

            string token = File.ReadLines("Token.txt").First(); // Remember to keep this private!
            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
            SetBotDetails();
            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content.ToLower() == "~ping")
            {
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync("Pong~");
            }
            else if (message.Content.ToLower().StartsWith("~add "))
            {
                string WoTName = message.Content.ToLower().Replace("~add ", "");
                WoTUser UserObj = new WoTUser();
                try
                {
                    await UserObj.AddNewWoTUser(message.Author, WoTName);
                    using (StreamWriter writer = File.CreateText("Users/" + UserObj.DiscordId.ToString()))
                    {
                        await writer.WriteAsync(JsonConvert.SerializeObject(UserObj, Formatting.Indented));
                    }
                    await message.Channel.SendMessageAsync("Done!");
                }
                catch
                {
                    await message.Channel.SendMessageAsync("Failed! Seems like that user doesn't exist :(");
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
            string Json = File.ReadAllText("Users/" + InitiatorMessage.Author.Id.ToString());
            WoTUser WoTUserObj = JsonConvert.DeserializeObject<WoTUser>(Json);
            string PlayerTreesCut = WoTUserObj.WotPlayerData["data"][WoTUserObj.WoTID.ToString()]["statistics"]["trees_cut"].ToString();
            string PlayerNickname = WoTUserObj.WotPlayerData["data"][WoTUserObj.WoTID.ToString()]["nickname"].ToString();
            InitiatorMessage.Channel.SendMessageAsync("Hello " + PlayerNickname + ", you have cut down " + PlayerTreesCut + " trees!");
            return Task.CompletedTask;
        }

        private async Task WoTGetSignature(SocketMessage InitiatorMessage)
        {
            string Json = File.ReadAllText("Users/" + InitiatorMessage.Author.Id.ToString());
            WoTUser WoTUserObj = JsonConvert.DeserializeObject<WoTUser>(Json);
            string Filename = "Users/" + WoTUserObj.WoTID + ".png";
            using (WebClient wc = new WebClient())
            {
                wc.DownloadFile("http://wotlabs.net/sig_cust/FFFFFF/36393e/eu/25g/200g/1000g/" + WoTUserObj.WotPlayerData["data"][WoTUserObj.WoTID.ToString()]["nickname"].ToString() + "/signature.png", Filename);
                await InitiatorMessage.Channel.SendFileAsync(Filename);
                File.Delete(Filename);
            }
        }

        private async Task SetBotDetails()
        {
            if (!File.Exists("icon.png"))
            {
                new WebClient().DownloadFile("https://worldoftanks.asia/dcont/fb/image/image_gupmod_01.png", "icon.png");
            }
            FileStream fileStreamer = new FileStream("icon.png", FileMode.Open);
            await _client.CurrentUser.ModifyAsync(x => x.Avatar = new Image(fileStreamer));
        }

        private static async Task<string> ReadTextAsync(string filePath)
        {
            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Open, FileAccess.Read, FileShare.Read,
                bufferSize: 4096, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();

                byte[] buffer = new byte[0x1000];
                int numRead;
                while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                {
                    string text = Encoding.Unicode.GetString(buffer, 0, numRead);
                    sb.Append(text);
                }

                return sb.ToString();
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
