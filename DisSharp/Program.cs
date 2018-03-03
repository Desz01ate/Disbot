using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.VoiceNext;
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
        static List<DiscordUserStamp> loggedInUser = new List<DiscordUserStamp>();
        static List<DiscordMessage> waitForDeleteMessage = new List<DiscordMessage>();
        static bool isAlerted = false;
        static int counter = 0;
        static void Main(string[] args)
        {
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
            if (!Directory.Exists($@"{AppDomain.CurrentDomain.BaseDirectory}/preferences/"))
            {
                Directory.CreateDirectory($@"{AppDomain.CurrentDomain.BaseDirectory}/preferences/");
                //File.Create($@"{AppDomain.CurrentDomain.BaseDirectory}/preferences/config.json");   
                File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/preferences/config.json"
                    , JsonConvert.SerializeObject(new { Token = "Your token here", DebugMode = LogLevel.Info, BotChannelID = 0, TextChannelID = 0, CommandPrefix = "!" }));
            }
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            try
            {
                discord = new DiscordClient(new DiscordConfiguration
                {
                    Token = BotConfig.GetContext.Token,
                    TokenType = TokenType.Bot,
                    UseInternalLogHandler = true,
                    LogLevel = BotConfig.GetContext.DebugMode,

                });

                discord.ClientErrored += async delegate
                {
                    Console.WriteLine("Error Triggered");
                    await discord.ReconnectAsync();
                };
                discord.VoiceStateUpdated += VoiceStateUpdatedEvent;
                discord.Ready += GetReady;
                discord.Heartbeated += HeartBeatedEvent;
                discord.MessageCreated += MessageCreateEvent;
                commands = discord.UseCommandsNext(new CommandsNextConfiguration
                {
                    StringPrefix = "!"
                });
                commands.RegisterCommands<Commands>();
                await discord.ConnectAsync();
                await Task.Delay(-1);
            }
            catch
            {
                Console.WriteLine($@"Please config your bot credential at {AppDomain.CurrentDomain.BaseDirectory}preferences\config.json first before attempt to launch this application.");
            }
        }

        private static async Task MessageCreateEvent(MessageCreateEventArgs e)
        {
            var creator = e.Author;
            var isCommand = false;
            try
            {
                isCommand = e.Message.Content.Substring(0, 1) == BotConfig.GetContext.CommandPrefix;
            }
            catch
            {
                Console.WriteLine($@"[{DateTime.Now}] Content is an image, skipping.");
            }
            // if (creator == discord.CurrentUser && e.Channel.Id == BotConfig.GetContext.TextChannelID)
            if (isCommand || (creator == discord.CurrentUser && e.Channel.Id == BotConfig.GetContext.TextChannelID))
            {
                waitForDeleteMessage.Add(e.Message);
            }
            await Task.Delay(1);
        }

        private static async Task VoiceStateUpdatedEvent(VoiceStateUpdateEventArgs e)
        {
            var ch = await discord.GetChannelAsync(BotConfig.GetContext.TextChannelID);
            if (e.Channel.Name == ch.Guild.Channels[4].Name && !loggedInUser.Exists(x => { return x.user == e.User && DateTime.Now.Date == x.stamp.Date; }))
            {
                await ch.SendMessageAsync($@"ยินดีต้อนรับกลับสู่ {ch.Guild.Channels[4].Name}, {e.User.Mention}!");
                loggedInUser.Add(new DiscordUserStamp() { user = e.User, stamp = DateTime.Now });
            }

        }

        private static async Task GetReady(ReadyEventArgs e)
        {
            /*
            var vch = discord.UseVoiceNext();
            await vch.ConnectAsync(await discord.GetChannelAsync(352734158885224448));
            */
            var ch = await discord.GetChannelAsync(BotConfig.GetContext.TextChannelID);
            await ch.SendMessageAsync($@"{discord.CurrentUser.Mention} มาแล้ว! มีคำถามสงสัย กด !help ได้เลยนะจ๊ะ d(￣◇￣)b");

        }

        private static async Task HeartBeatedEvent(HeartbeatEventArgs e)
        {
            if(Commands.bossList.Count > 0)
            {
                var isExtended = false;
                var boss = Commands.bossList[0];
                var spawnTime = (boss.time.AddHours(boss.window)) - DateTime.Now;
                var ch = await discord.GetChannelAsync(BotConfig.GetContext.BotChannelID);
                if (spawnTime.Hours < 0 || spawnTime.Minutes < 0)
                {
                    spawnTime = (boss.time.AddHours(boss.window).AddHours(boss.extend)) - DateTime.Now;
                    isExtended = true;
                    if (spawnTime.Hours < 0 || spawnTime.Minutes < 0)
                    {

                        BossCalibrate(boss.name);
                        isExtended = false;
                        await discord.SendMessageAsync(ch, $@"@everyone ไม่มีใครรายงานเวลาเกิดบอสจนหมดรอบ 12 ชั่วโมงแล้ว ขอ Recalibrate บอสก่อนนะ ถ้ามากันแล้ว มาเซ็ทเวลาใหม่ด้วย!");
                    } //even after adding the extend still out of scope then something went wrong
                }
                var returnString = string.Empty;
                if (!isExtended)
                {
                    isAlerted = false;
                }
                else
                {
                    if (!isAlerted)
                    {
                        await discord.SendMessageAsync(ch, $@"@everyone {boss.name} อยู่ในช่วงรอเกิดแล้ว!");
                        isAlerted = true;
                    }
                }
                var prefix = isExtended ? "[*]" : string.Empty;
                await discord.UpdateStatusAsync(new DiscordGame() { Name = $@"{prefix}Remaining {spawnTime.Hours.ToString().PadLeft(2, '0')} h {spawnTime.Minutes.ToString().PadLeft(2, '0')} m" });
                // remove chat
                if (counter == 2)
                {
                    waitForDeleteMessage.ForEach(async m =>
                    {
                        try
                        {
                            await m.DeleteAsync();
                        }
                        catch
                        {
                            Console.WriteLine("Message is not found.");
                        }
                        waitForDeleteMessage.Remove(m);
                        await Task.Delay(300); // 3 requests per second
                    });
                    counter = 0;
                }
                else
                    counter++;
            }

        }

        private static void BossCalibrate(string name)
        {
            var bossIndex = Commands.bossList.FindIndex(x => { return x.name == name; });
            var boss = Commands.bossList[bossIndex];
            Console.WriteLine("Recalibrating...");
            Commands.bossList[bossIndex].time = DateTime.Now;
            File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/bosses/{boss.name}.json", JsonConvert.SerializeObject(boss));
            Console.WriteLine("Calibrated.");
            
        }
    }


}
