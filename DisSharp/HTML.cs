using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Disbot
{
    static class HTML
    {
        public static string Image_source
        {
            get
            {
                return "<img.+?src=[\"'](.+?)[\"'].+?>";
            }
        }
        public static string A_hyperlink_reference
        {
            get
            {
                return "<a.+?href=[\"'](.+?)[\"'].+?>";
            }
        }
        public static async Task<List<string>> HTMLParser(string searchingKeyword,string regexPattern = "<img.+?src=[\"'](.+?)[\"'].+?>")
        {
            var url = $@"https://www.google.co.th/search?q={searchingKeyword}&source=lnms&tbm=isch&sa=X&ved=0ahUKEwjl0-jRz8zZAhUIjJQKHRhpAwQQ_AUICigB&biw=1065&bih=557";
            var ResultList = new List<string>();
            try
            {

                HttpClient http = new HttpClient();
                var response = await http.GetByteArrayAsync(url);
                var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                var result = Regex.Matches(source,regexPattern, RegexOptions.IgnoreCase);

                for (var i = 0; i < result.Count; i++)
                {
                    ResultList.Add(result[i].Groups[1].Value); //get src group from img
                }
            }
            catch
            {
                throw new Exception();
            }
            return ResultList;
        }
        public static async Task<string> GetUncyclopedia(string searchingKeyword)
        {
            var url = $@"http://th.uncyclopedia.info/wiki/{searchingKeyword}";
            try
            {
                HttpClient http = new HttpClient();
                var response = await http.GetByteArrayAsync(url);
                var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                if (!source.Contains("ไม่มีในไร้สาระนุกรม"))
                    return url;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return $@"https://www.google.co.th/search?q={searchingKeyword}";
        }
    }
}
