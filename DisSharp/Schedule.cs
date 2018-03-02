using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SQLite;
namespace Disbot
{
    class Schedule
    {
        [PrimaryKey]
        public int sd_id { get; set; }
        public string sd_title { get; set; }
        public ulong sd_owner_id { get; set; }
        public DateTime sd_publish_date { get; set; }
        Schedule()
        {
            var path = $@"{AppDomain.CurrentDomain.BaseDirectory}/scheduledb.db";
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            var sqlite = new SQLite.SQLiteConnection(path);
            sqlite.CreateTable<Schedule>();
        }
    }
}
