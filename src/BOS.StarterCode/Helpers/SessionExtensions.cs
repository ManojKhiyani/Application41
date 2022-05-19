using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Linq;
using System.Text.RegularExpressions;
namespace BOS.StarterCode.Helpers
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
        public static string ToInitials(this string str)
        {
            string strSplit = "";
            str = str.Trim();
            str.Split(" ").ToList().ForEach(i => strSplit = strSplit + "" + i[0]) ;
            return strSplit;
        }
    }
}
