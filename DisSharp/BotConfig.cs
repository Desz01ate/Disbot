using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace DisSharp
{
    class BotConfig
    {
        private static readonly BotConfig _bot = new BotConfig();
        public string Token { get; set; }
        public ulong TextChanneID { get; set; }
        public LogLevel DebugMode { get; set; }
        public static BotConfig GetContext { get { return _bot; } }
        private BotConfig()
        {

        }
    }
}
