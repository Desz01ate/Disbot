using Disbot;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DisSharp
{
    public class Commands
    {
        public static List<Boss> bossList = new List<Boss>() {
            new Boss(){
                name = "kzarka",
                time = DateTime.Now,
                window = 8,
                extend = 4
            }
        }; //would be replace if the json files is exists

        [Command("กวาดบ้าน")]
        [Description("เก็บกวาดอะไรที่มันรกหูรกตา")]
        public async Task Cleaning(CommandContext ctx)
        {
            await ctx.RespondAsync("กวาดแปบ");
            var message = await (await ctx.Client.GetChannelAsync(BotConfig.GetContext.BotChannelID)).GetMessagesAsync();
            for (var index = 0; index < message.Count; index++)
            {
                await message[index].DeleteAsync();
            }
            await ctx.RespondAsync("เสร็จแล้ว");
        }
        [Command("ขอวาป")]
        [Description("ส่งคุณไปยังที่ดีๆ ตามคำค้นที่อยากไป")]
        public async Task GETUrl(CommandContext ctx, string keyword)
        {
            var URL = await HTML.GetUncyclopedia(keyword);
            //URL = URL.GetRange(0, 4); //get only first three images, comment this line to show all images.
            if (URL.Contains("pedia"))
                await ctx.RespondAsync($@"ลองเข้าไปอ่าน {URL} ดู!");
            else
                await ctx.RespondAsync($@"คีย์เวิร์ดโคตรกาก! ไปอ่านแถวนี้ดูแล้วกัน {URL}");
        }
        [Command("ขอดูรูป")]
        [Description("หารูปมาให้ดูเก๋ๆ ตามคำค้นที่คุณอยากได้")]
        public async Task test(CommandContext ctx, string keyword)
        {
            var imgSrc = await HTML.HTMLParser(keyword, HTML.Image_source);
            imgSrc = imgSrc.GetRange(0, 4); //get only first three images, comment this line to show all images.
            imgSrc.ForEach(async image =>
            {
                try
                {
                    /*
                    var webClient = new WebClient();
                    var fileName = image.Substring(image.Length - 4, 4) + ".jpg";
                    var fullPath = $@"{AppDomain.CurrentDomain.BaseDirectory}/img/{fileName}";
                    webClient.DownloadFile(image, fullPath);
                    using (var stream = File.OpenRead(fullPath))
                    {
                        await ctx.RespondWithFileAsync(stream, fileName);
                    }
                    File.Delete(fullPath);*/
                    await ctx.RespondAsync(image);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"[{DateTime.Now}]get image module causing {ex.Message}");
                }
            });
        }
        [Command("setboss")]
        [Description("ตั้งเวลาบอสตาย ณ ปัจจุบัน หรือ เวลาที่กำหนดได้ แต่ต้องได้รับอนุญาตก่อน ถึงจะมีสิทธิ์ตั้งได้ d(ﾟｰﾟ@)")]
        [Cooldown(1,300,CooldownBucketType.User)]
        [RequirePermissions(DSharpPlus.Permissions.Administrator)]
        public async Task SetBoss(CommandContext ctx, string bossName, string hour = null, string min = null)
        {
            WriteLog(ctx);
            // Is not an actual bosses
            var boss = GetBossByName(bossName);
            if (boss == null)
            {
                await ctx.RespondAsync($"เค้าไม่รู้จักตัวนี้น้า ดูใหม่อีกที~");
                return;
            }
            var time = DateTime.Now;
            if (hour != null && (int.Parse(hour) >= 0 && int.Parse(hour) <= 23) && min != null && (int.Parse(min) >= 0 && int.Parse(min) <= 59))
                time = boss.time = DateTime.ParseExact($@"{hour.PadLeft(2, '0')}:{min.PadLeft(2, '0')}:00", "HH:mm:ss", CultureInfo.InvariantCulture);
            boss.time = time;
            await ctx.RespondAsync($"ตั้งเวลาเกิดให้ {boss.name} แล้วจ้ะ (*♡∀♡)");
            bossList.ForEach(b =>
            {
                File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/bosses/{b.name}.json", JsonConvert.SerializeObject(b));
            });
            return;
            /*
            ulong[] whiteList = JsonConvert.DeserializeObject<ulong[]>(File.ReadAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/preferences/whitelist.json"));
            for (var i = 0; i < whiteList.Length; i++)
            {
                if (whiteList[i] == ctx.User.Id)
                {
                    var time = DateTime.Now;
                    if (hour != null && (int.Parse(hour) >= 0 && int.Parse(hour) <= 23) && min != null && (int.Parse(min) >= 0 && int.Parse(min) <= 59))
                        time = boss.time = DateTime.ParseExact($@"{hour.PadLeft(2, '0')}:{min.PadLeft(2, '0')}:00", "HH:mm:ss", CultureInfo.InvariantCulture);
                    boss.time = time;
                    await ctx.RespondAsync($"ตั้งเวลาเกิดให้ {boss.name} แล้วจ้ะ (*♡∀♡)");
                    bossList.ForEach(b =>
                    {
                        File.WriteAllText($@"{AppDomain.CurrentDomain.BaseDirectory}/bosses/{b.name}.json", JsonConvert.SerializeObject(b));
                    });
                    return;
                }
            }
            await ctx.RespondAsync($"{ctx.User.Mention} ไม่มีสิทธิ์ตั้งค่าเวลาบอสนะ ลองติดต่อ {(await ctx.Client.GetUserAsync(322051347505479681)).Mention} ดู");
            */

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
        [Description("ขอดูเวลาบอสที่ต้องการได้นะ (￣ー￣)ｂ")]
        [Cooldown(3, 600, CooldownBucketType.User)]
        public async Task GetBoss(CommandContext ctx, string bossName)
        {
            /*
            await ctx.RespondAsync($@"ฟังก์ชันนี้ปิดการทำงานอยู่จ้ะ รอการอัพเดทนะ (-。-;");
            return;
            */
            //disable
            WriteLog(ctx);
            var isExtended = false;
            var ch = await ctx.Client.GetChannelAsync(BotConfig.GetContext.BotChannelID);
            var boss = GetBossByName(bossName);
            if (boss != null)
            {
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
                    returnString = $"{ctx.User.Mention} บอส [{boss.name.ToUpper()}] จะ IN WINDOW ใน  {spawnTime.Hours} ชั่วโมง {spawnTime.Minutes} นาที♪";
                }
                else
                {
                    //await ctx.Client.UpdateStatusAsync(new DiscordGame() { Name = $@"{boss.name.ToUpper()} until {DateTime.Now.Add(spawnTime).ToString("HH:mm tt")}" });
                    returnString = $"{ctx.User.Mention} บอส [{boss.name.ToUpper()}] อยู่ในช่วง EXTEND WINDOW อีก {spawnTime.Hours} ชั่วโมง {spawnTime.Minutes} นาที♪";
                }
                await ch.SendMessageAsync(returnString);

            }
            else
            {
                await ctx.RespondAsync($@"หาบอสชื่อนี้ไม่เจอจ้า (-。-;");
            }
        }
        [Command("สวัสดี")]
        [Description("ทักทายกันไง! (*＾▽＾)／")]
        public async Task Hi(CommandContext ctx)
        {
            WriteLog(ctx);
            await ctx.RespondAsync($"👋 หวัดดี, {ctx.User.Mention}!");

        }
        [Command("สุ่มเลข")]
        [Description("เล่นสุ่มเลขกันหน่อยป่าว （´ヘ｀；")]
        public async Task Random(CommandContext ctx, int min, int max)
        {
            WriteLog(ctx);
            var rnd = new Random();
            await ctx.RespondAsync($"🎲 เราสุ่มเลขให้นาย ได้: {rnd.Next(min, max)}");
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
