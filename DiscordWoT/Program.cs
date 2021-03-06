﻿using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
#if !NETCOREAPP2_0
using Discord.Net.Providers.WS4Net;
#endif
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordWoT
{
    partial class Program
    {
#if NETCOREAPP2_0
        public static string VersionString = "0.1.10 .NET Core";
#else
        public static string VersionString = "0.1.10 .NET";
#endif
        public static string WargammingKey;
        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        private DiscordSocketClient _client;
        private CommandService commands;
        private IServiceProvider services;

        public async Task MainAsync()
        {
            Directory.CreateDirectory("Users");
            _client = new DiscordSocketClient(new DiscordSocketConfig
            {
#if !NETCOREAPP2_0
                WebSocketProvider = WS4NetProvider.Instance
#endif
            });
            commands = new CommandService();

            services = new ServiceCollection().BuildServiceProvider();

            _client.Log += Log;
            _client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());

            string DiscordToken = File.ReadLines(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Token.txt").First(); // Remember to keep this private!
            Program.WargammingKey = File.ReadLines(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "/Token.txt").Last();
            await _client.LoginAsync(TokenType.Bot, DiscordToken);
            await _client.StartAsync();
            SetBotDetails();
            // Block this task until the program is closed.
            await Task.Delay(-1);
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

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
