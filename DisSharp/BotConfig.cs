using DSharpPlus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DisSharp
{
    public sealed class BotConfig
    {
        static readonly BotConfig _botInstance = new BotConfig();
        public string Token { get;  set; }
        public ulong TextChannelID { get;  set; }
        public ulong BotChannelID { get; set; }
        public LogLevel DebugMode { get;  set; }
        public string CommandPrefix { get; set; }
        public string MongoDBConnectionString { get; set; }
        public static BotConfig GetContext => _botInstance;
        static BotConfig()
        {
            var botConfig = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/preferences/config.json"));
            _botInstance.Token = botConfig.Token;
            _botInstance.TextChannelID = botConfig.TextChannelID;
            _botInstance.BotChannelID = botConfig.BotChannelID;
            _botInstance.DebugMode = botConfig.DebugMode;
            _botInstance.CommandPrefix = botConfig.CommandPrefix;
            _botInstance.MongoDBConnectionString = botConfig.MongoDBConnectionString;
        }
 
    }
}
