using BOS.Base.Client;
using Multitenancy.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BOS.StarterCode.Models.BOSModels
{
    public class Product : IProduct
    {
        public Guid Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool Deleted { get; set; }
        [JsonProperty("TenantUriClaims", ItemConverterType = typeof(ConcreteConverter<ITenantUriClaims, TenantUriClaims>))]
        public List<ITenantUriClaims> TenantUriClaims { get; set; }
    }

    public class ProductClaim : IProductClaims
    {
        public string Token { get; set; }
        public string URL { get; set; }
        public string ConnectionString { get; set; }
        public Guid AccountId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ComponentId { get; set; }
        public Guid TenantId { get; set; }
        public Guid InstanceId { get; set; }
        public string IssuedAt { get; set; }
        public string ExpiresBy { get; set; }
    }
}
