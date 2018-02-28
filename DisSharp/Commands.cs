using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DisSharp
{
    public class Commands
    {
        private static readonly int requiredReport = 2;
        private static readonly int forceReport = 5;
        public static List<Boss> bossList = new List<Boss>() {
            new Boss(){
                name = "kzarka",
                time = DateTime.Now,
                window = 8,
                extend = 4
            }
        }; //would be replace if the json files is exists
        private static List<string> reporterList = new List<string>();
        private static int reportCouter = 0;
        private static int forceReportCounter = 0;
        [Command("gethelp")]
        public async Task Help(CommandContext ctx)
        {
            WriteLog(ctx);
            var helpText =
@"!getboss {boss_name} : ขอดูเวลาบอสที่ต้องการได้นะ (￣ー￣)ｂ(ปิดใช้งานอยู่นะ ดูเวลาที่ข้อมูลของโอ้ทางขวาได้เลย!)

!setboss {boss_name} : ตั้งเวลาบอสที่ต้องการได้ แต่ต้องใช้ 2 คนช่วยกันนะ ถึงจะตั้งได้ d(ﾟｰﾟ@)

!forcesetboss {boss_name} {hour} {minute} : ใช้ตั้งเวลาบอสในกรณีที่ไม่ได้ !setboss ให้ทันเวลาได้ แต่ใช้ 5 คนช่วยกันนะ (*TーT)b

!hi : ทักทายกันไง! (*＾▽＾)／

!random {min} {max} : เล่นสุ่มเลขกันหน่อยป่าว （´ヘ｀；）
                ";
            var dm = await ctx.Client.CreateDmAsync(ctx.User);
            await dm.SendMessageAsync(helpText);
        }
        [Command("setboss")]
        public async Task SetBoss(CommandContext ctx, string bossName)
        {
            WriteLog(ctx);
            // Is not an actual bosses
            var boss = GetBossByName(bossName);
            if (boss == null)
            {
                await ctx.RespondAsync($"เค้าไม่รู้จักตัวนี้น้า ดูใหม่อีกที~");
                return;
            }

            // Reject duplicate reporter 
            if (reporterList.Contains(ctx.User.Username))
            {
                await ctx.RespondAsync($@"อย่ารีพอตบอสซ้ำๆ สิ เดี๋ยวแบนเลย ヾ(`ヘ´)ﾉﾞ ");
                return;
            }

            // Accept report
            if (reportCouter < requiredReport && !reporterList.Contains(ctx.User.Username))
            {
                reportCouter++;
                reporterList.Add(ctx.User.Username);
                await ctx.RespondAsync($"เค้ารอคนรายงานเพิ่มอีก {requiredReport - reportCouter} คน แล้วเดี๋ยวเค้าตั้งเวลาเกิดบอสให้น้า (─‿‿─)♡");
            }
            // Boss is in the list and the vote is equal to or above 5
            if (reportCouter >= requiredReport)
            {
                reportCouter = 0;
                reporterList.Clear();
                boss.time = DateTime.Now;
                await ctx.Client.UpdateStatusAsync(new DiscordGame() { Name = $@"{boss.name.ToUpper()} In {boss.time.AddHours(boss.window).ToString("HH:mm tt")}" });
                await ctx.RespondAsync($"ตั้งเวลาเกิดให้ {boss.name} แล้วจ้ะ (*♡∀♡)");
                bossList.ForEach(b =>
                {
                    File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/bosses/{b.name}.json", JsonConvert.SerializeObject(b));
                });
            }

        }

        private Boss GetBossByName(string bossName)
        {
            foreach (var b in bossList)
            {
                if (b.name.Contains(bossName.ToLower()))
                {
                    return b;
                }
            }
            return null;
        }
        [Command("getboss")]
        public async Task GetBoss(CommandContext ctx, string bossName)
        {
            await ctx.RespondAsync($@"ฟังก์ชันนี้ปิดการทำงานอยู่จ้ะ รอการอัพเดทนะ (-。-;");
            return;
            //disable
            WriteLog(ctx);
            var isExtended = false;
            var isFound = false;
            bossList.ForEach(async boss =>
            {
                if (boss.name == bossName.ToLower())
                {
                    isFound = true;
                    var spawnTime = (boss.time.AddHours(boss.window)) - DateTime.Now;
                    if (spawnTime.Hours < 0 || spawnTime.Minutes < 0)
                    {
                        spawnTime = (boss.time.AddHours(boss.window).AddHours(boss.extend)) - DateTime.Now;
                        isExtended = true;
                    }
                    var returnString = string.Empty;
                    if (!isExtended)
                    {
                        //await ctx.Client.UpdateStatusAsync(new DiscordGame() { Name = $@"{boss.name.ToUpper()} In {DateTime.Now.Add(spawnTime).ToString("HH:mm tt")}" });
                        returnString = $"บอส [{boss.name.ToUpper()}] จะเกิดใน {spawnTime.Hours} ชั่วโมง {spawnTime.Minutes} นาที ก็คือ {DateTime.Now.Add(spawnTime).ToString("HH:mm tt")} น่ะจ้ะ ♪";
                    }
                    else
                    {
                        //await ctx.Client.UpdateStatusAsync(new DiscordGame() { Name = $@"{boss.name.ToUpper()} until {DateTime.Now.Add(spawnTime).ToString("HH:mm tt")}" });
                        returnString = $"บอส [{boss.name.ToUpper()}] เหลือเวลาช่วงสุ่มเกิดในอีก {spawnTime.Hours} ชั่วโมง {spawnTime.Minutes} นาที ก็คือ {DateTime.Now.Add(spawnTime).ToString("HH:mm tt")} น่ะจ้ะ ♪";
                    }
                    await ctx.RespondAsync(returnString);

                }
            });
            if (!isFound)
            {
                await ctx.RespondAsync($@"หาบอสชื่อนี้ไม่เจอจ้า (-。-;");
            }
        }
        [Command("hi")]
        public async Task Hi(CommandContext ctx)
        {
            WriteLog(ctx);
            await ctx.RespondAsync($"👋 หวัดดี, {ctx.User.Mention}!");

        }
        [Command("random")]
        public async Task Random(CommandContext ctx, int min, int max)
        {
            WriteLog(ctx);
            var rnd = new Random();
            await ctx.RespondAsync($"🎲 เราสุ่มเลขให้นาย ได้: {rnd.Next(min, max)}");
        }
        [Command("forcesetboss")]
        public async Task ForceSetBoss(CommandContext ctx, string bossName, string hour, string min)
        {
            WriteLog(ctx);
            // Is not an actual bosses
            var boss = GetBossByName(bossName);
            if (boss == null)
            {
                await ctx.RespondAsync($"เค้าไม่รู้จักตัวนี้น้า ดูใหม่อีกที~");
                return;
            }

            // Reject duplicate reporter 
            if (reporterList.Contains(ctx.User.Username))
            {
                await ctx.RespondAsync($@"อย่ารีพอตบอสซ้ำๆ สิ เดี๋ยวแบนเลย ヾ(`ヘ´)ﾉﾞ ");
                return;
            }

            // Accept report
            if (forceReportCounter < forceReport && !reporterList.Contains(ctx.User.Username))
            {
                forceReportCounter++;
                reporterList.Add(ctx.User.Username);
                await ctx.RespondAsync($"เค้ารอคนรายงานเพิ่มอีก {forceReport - forceReportCounter} คน แล้วเดี๋ยวเค้าตั้งเวลาเกิดบอสให้น้า (─‿‿─)♡");
            }
            // Boss is in the list and the vote is equal to or above 5
            if (forceReportCounter >= forceReport)
            {
                forceReportCounter = 0;
                reporterList.Clear();
                boss.time = DateTime.ParseExact($@"{hour.PadLeft(2, '0')}:{min.PadLeft(2, '0')}:00", "HH:mm:ss", CultureInfo.InvariantCulture);
                await ctx.RespondAsync($"ตั้งเวลาเกิดให้ {boss.name} แล้วจ้ะ (*♡∀♡)");
                bossList.ForEach(b =>
                {
                    File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/bosses/{b.name}.json", JsonConvert.SerializeObject(b));
                });
            }
        }
        public void WriteLog(CommandContext ctx)
        {
            Console.WriteLine($@"[{DateTime.Now}]:Get request from {ctx.User}");
        }
        public static string GetTime()
        {
            return $@"{(DateTime.Now.Hour % 12).ToString().PadLeft(2, '0')}:{DateTime.Now.Minute.ToString().PadLeft(2, '0')}:{DateTime.Now.Second.ToString().PadLeft(2, '0')} {DateTime.Now.ToString("tt", CultureInfo.InvariantCulture)}";
        }
    }
}
