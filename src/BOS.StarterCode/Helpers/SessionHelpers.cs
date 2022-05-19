using BOS.Auth.Client;
using BOS.Email.Client;
using BOS.IA.Client;
using BOS.StarterCode.Models;
using BOS.StarterCode.Models.BOSModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Multitenancy.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
namespace BOS.StarterCode.Web.Helpers
{
    public class SessionHelpers
    {
        private IConfiguration _configuration;
        private readonly IDistributedCache _distributedCache;
        private readonly IHttpContextAccessor _contextAccessor;
        public IMultitenancyClient _bosMultitenancyClient;
        private HttpClient _httpClient = new HttpClient();
        public SessionHelpers()
        {
        }
        public SessionHelpers(IConfiguration configuration, IDistributedCache distributedCache,
        IHttpContextAccessor contextAccessor)
        {
            _configuration = configuration;
            _distributedCache = distributedCache;
            _contextAccessor = contextAccessor;
            SetMultitenancyClient();
        }

        public async Task<TokenResponse> GetGeneratedToken()
        {
            TokenResponse response = new TokenResponse();
            try
            {
                bool isFetch = true;
                var token = _contextAccessor.HttpContext.Session.GetString("ApplicationToken");
                if (token != null)
                {
                    response = JsonConvert.DeserializeObject<TokenResponse>(token);
                    if (response != null)
                    {
                        isFetch = false;
                    }
                }
                if (isFetch)
                {
                    string host_url = _contextAccessor.HttpContext.Request.Host.ToString();
                    string appPath = string.Format("{0}://{1}", host_url.Contains("localhost") ? "http" : "https", host_url);
                    string clientId = _configuration["AppCredentials:ClientId"];
                    string clientSecret = _configuration["AppCredentials:ClientSecret"];
                    var jsonResult = await _bosMultitenancyClient.ValidateURLClientIdClientSecretAsync<ProductClaim>(clientId, clientSecret, appPath);
                    if (jsonResult != null)
                    {
                        if (jsonResult?.ProductClaims?.Token != null)
                        {
                            response.data = jsonResult?.ProductClaims?.Token;
                            response.ConnectionString = jsonResult?.ProductClaims?.ConnectionString;
                            response.message = "Token Generated";
                        }
                        else if (jsonResult.BOSErrors != null && jsonResult.BOSErrors.Any(sd => sd.Message.Contains("passed is incorrect.")))
                        {
                            response.message = "ClientId or ClientSecret is incorrect";
                        }
                        else if (jsonResult.BOSErrors != null && jsonResult.BOSErrors.Any(sd => sd.Message.Contains("does not exist.")))
                        {
                            response.message = "Website Url does not exists in the system";
                        }
                        if (!string.IsNullOrEmpty(response.data))
                        {
                            _contextAccessor.HttpContext.Session.SetString("ApplicationToken", JsonConvert.SerializeObject(response));
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return response;
        }

        public IActionResult SetMultitenancyClient()
        {
            try
            {
                string bosServiceURL = _configuration["BOS:ServiceBaseURL"];
                var client = new HttpClient();
                client.BaseAddress = new Uri("" + bosServiceURL + _configuration["BOS:MultiTenancyRelativeURL"]);
                _bosMultitenancyClient = new MultitenancyClient(client, "", _distributedCache);
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
