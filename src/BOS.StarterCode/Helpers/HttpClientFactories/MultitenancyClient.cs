using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System;
using BOS.StarterCode.Models.BOSModels;

namespace BOS.StarterCode.Helpers.HttpClientFactories
{
    public partial class ApiClient
    {
        public List<WhiteLabel> GetApplicationConfiguration(string baseurl)
        {
            List<WhiteLabel> response = new List<WhiteLabel>();
            if (!string.IsNullOrEmpty(baseurl))
            {
                Uri appUri = new Uri(baseurl);
                string appPath = string.Format("{0}://{1}{2}", appUri.Scheme, appUri.Host, (appUri.Host == "localhost" ? ":"+appUri.Port.ToString() : ""));
                string queryString = "?$filter=URL eq '" + appPath + "'&api-version=1.0";
                var requestUrl = CreateRequestUri(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                    "WhiteLabels"), queryString);
                try
                {
                    var jsonResult =  GetSync<JObject>(requestUrl);
                    if (jsonResult["value"] != null && jsonResult["value"].Count() > 0)
                    {
                        response = JsonConvert.DeserializeObject<List<WhiteLabel>>(jsonResult["value"].ToString());
                    }
                }
                catch (Exception) { }
            }
            return response;
        }
    }
}
