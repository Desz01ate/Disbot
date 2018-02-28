using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DisSharp
{
    class DiscordUserStamp
    {
        public DiscordUser user { get; set; }
        public DateTime stamp { get; set; }
    }
    class Program
    {
        static DiscordClient discord;
        static CommandsNextModule commands;
        static BotConfig botConfig;
        static List<DiscordUserStamp> loggedInUser = new List<DiscordUserStamp>();
        static void Main(string[] args)
        {
            botConfig = JsonConvert.DeserializeObject<BotConfig>(File.ReadAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/config.json"));
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}/bosses/"))
            {
                Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}/bosses/");
            }
            else
            {
                Commands.bossList.Clear();
                string[] bossFiles = Directory.GetFiles($@"{AppDomain.CurrentDomain.BaseDirectory}/bosses/", "*.json");
                for (var file = 0; file < bossFiles.Length; file++)
                {
                    var jsonString = File.ReadAllText(bossFiles[file]);
                    var currentBoss = JsonConvert.DeserializeObject<Boss>(jsonString);
                    Commands.bossList.Add(currentBoss);
                }
            }
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        static async Task MainAsync(string[] args)
        {
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = botConfig.DebugMode,

            });
            try
            {
                await discord.DisconnectAsync(); //try disconnect first
            }
            catch { }

            discord.VoiceStateUpdated += VoiceStateUpdatedEvent;
            discord.Ready += GetReady;
            discord.Heartbeated += HeartBeatedEvent;
            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefix = "!"
            });
            commands.RegisterCommands<Commands>();
            await discord.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task VoiceStateUpdatedEvent(VoiceStateUpdateEventArgs e)
        {
            var ch = await discord.GetChannelAsync(botConfig.TextChanneID);
            if (e.Channel.Name == ch.Guild.Channels[4].Name && !loggedInUser.Exists(x => { return x.user == e.User && DateTime.Now.Date == x.stamp.Date; }))
            {
                await ch.SendMessageAsync($@"ยินดีต้อนรับกลับสู่ {ch.Guild.Channels[4].Name}, {e.User.Mention}!");
                loggedInUser.Add(new DiscordUserStamp() { user = e.User, stamp = DateTime.Now });
            }
        }

        private static async Task GetReady(ReadyEventArgs e)
        {
            var ch = await discord.GetChannelAsync(botConfig.TextChanneID);
            await ch.SendMessageAsync($@"{discord.CurrentUser.Mention} มาแล้ว! มีคำถามสงสัย กด !gethelp ได้เลยนะจ๊ะ d(￣◇￣)b");
        }

        private static async Task HeartBeatedEvent(HeartbeatEventArgs e)
        {
            var isExtended = false;
            var boss = Commands.bossList[0];
            var spawnTime = (boss.time.AddHours(boss.window)) - DateTime.Now;
            if (spawnTime.Hours < 0 || spawnTime.Minutes < 0)
            {
                spawnTime = (boss.time.AddHours(boss.window).AddHours(boss.extend)) - DateTime.Now;
                isExtended = true;
            }
            var returnString = string.Empty;
            if (!isExtended)
            {
                await discord.EditCurrentUserAsync("Kzarka [Not In Window]");
            }
            else
            {
                await discord.EditCurrentUserAsync("Kzarka [In Window]");
            }
            await discord.UpdateStatusAsync(new DiscordGame() { Name = $@"Remaining {spawnTime.Hours.ToString().PadLeft(2, '0')} h {spawnTime.Minutes.ToString().PadLeft(2, '0')} m" });
        }
    }

   
}
