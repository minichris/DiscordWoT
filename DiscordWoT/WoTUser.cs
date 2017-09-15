using Discord.WebSocket;
using Newtonsoft.Json;
using System.IO;

namespace DiscordWoT
{
    class WoTUser
    {
        public ulong Id;
        public string nickname;
        public string WoTName;

        public WoTUser(SocketUser DiscordUser, string GivenWoTName)
        {
            Id = DiscordUser.Id;
            nickname = DiscordUser.Username;
            WoTName = GivenWoTName;
        }
    }
}