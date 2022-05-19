using BOS.Base.Client;
using Multitenancy.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BOS.StarterCode.Models.BOSModels
{
    public class TenantUriClaims : ITenantUriClaims
    {
        public Guid Id { get; set; }
        public string TenantUri { get; set; }
        public string LocalhostUri { get; set; }
        public string BundleId { get; set; }
        public string ConnectionString { get; set; }
        public string OldBundleId { get; set; }
        public string OldUri { get; set; }
        public Guid AccountId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ComponentId { get; set; }
        public Guid TenantId { get; set; }
        public Guid InstanceId { get; set; }
        public bool Deleted { get; set; }
    }

    public class TenantUriClaimsList : ITenantUriClaimsList
    {
        [JsonProperty("TenantUriClaims", ItemConverterType = typeof(ConcreteConverter<ITenantUriClaims, TenantUriClaims>))]
        public List<ITenantUriClaims> TenantUriClaims { get; set; }
    }
}
