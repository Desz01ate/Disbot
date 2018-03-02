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
        public static async Task<List<string>> ImageSearch(string searchingKeyword)
        {
            var url = $@"https://www.google.co.th/search?q={searchingKeyword}&source=lnms&tbm=isch&sa=X&ved=0ahUKEwjl0-jRz8zZAhUIjJQKHRhpAwQQ_AUICigB&biw=1065&bih=557";
            var ResultList = new List<string>();
            try
            {

                HttpClient http = new HttpClient();
                var response = await http.GetByteArrayAsync(url);
                var source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                var result = Regex.Matches(source, "<img.+?src=[\"'](.+?)[\"'].+?>", RegexOptions.IgnoreCase);

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
    }
}
