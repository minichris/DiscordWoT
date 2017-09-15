using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Net.Providers.WS4Net;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

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
                UserObj.AddNewWoTUser(message.Author, WoTName);
                Console.WriteLine("Adding a new user.");
                using (StreamWriter writer = File.CreateText("Users/" + UserObj.DiscordId.ToString()))
                {
                    await writer.WriteAsync(JsonConvert.SerializeObject(UserObj, Formatting.Indented));
                }
                Console.WriteLine("Done adding a new user.");
                await message.DeleteAsync();
                await message.Channel.SendMessageAsync("Done!");
            }
        }

        private Task SaveUserData()
        {

            return Task.CompletedTask;
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
