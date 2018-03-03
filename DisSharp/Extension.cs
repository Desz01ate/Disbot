using DisSharp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Disbot
{
    public static class Extension
    {
        public static bool CreateJSONFile(this Boss boss)
        {
            try
            {
                var file = $@"{AppDomain.CurrentDomain.BaseDirectory}bosses\{boss.name}.json";
                File.WriteAllText(file, JsonConvert.SerializeObject(boss));
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;
        }
    }
}
