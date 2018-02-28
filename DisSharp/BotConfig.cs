﻿using DSharpPlus;
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
        public ulong TextChanneID { get;  set; }
        public LogLevel DebugMode { get;  set; }
        public static BotConfig GetContext => _botInstance;

        static BotConfig()
        {
            var botConfig = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/config.json"));
            _botInstance.Token = botConfig.Token;
            _botInstance.TextChanneID = botConfig.TextChanneID;
            _botInstance.DebugMode = botConfig.DebugMode;
        }
 
    }
}
