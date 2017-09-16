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

namespace DiscordWoT
{
    class Program
    {
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

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

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }

        private async Task MessageReceived(SocketMessage message)
        {
            if (message.Content.ToLower() == "!ping")
            {
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync("Pong!");
            }
            else if (message.Content.ToLower().StartsWith("!wotadd "))
            {
                string WoTName = message.Content.ToLower().Replace("!wotadd ", "");
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
            else if (message.Content.ToLower().StartsWith("!wotgetme"))
            {
                await WotGetMeMessage(message);
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
