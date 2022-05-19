using BOS.Base.Client;
using Multitenancy.Client.ClientModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BOS.StarterCode.Models.BOSModels
{
    public class WhiteLabel : IWhiteLabel
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public Guid InstanceId { get; set; }
        public Guid ProductId { get; set; }
        public string Name { get; set; }
        public string Logo { get; set; }
        public string ThemeCss { get; set; }
        public string CopyrightText { get; set; }
        public string Template { get; set; }
        public string JavaScriptUrl { get; set; }
        [JsonProperty("Urls", ItemConverterType = typeof(ConcreteConverter<IWhiteLabelUrl, WhiteLabelUrl>))]
        public List<IWhiteLabelUrl> Urls { get; set; }
        public bool Deleted { get; set; }
    }

    public class WhiteLabelUrl : IWhiteLabelUrl
    {
        public Guid Id { get; set; }
        public Guid ComponentId { get; set; }
        public string URL { get; set; }
        public string LocalhostURL { get; set; }
        public string BundleId { get; set; }
        public Guid WhiteLabelId { get; set; }
        public bool Deleted { get; set; }
    }
}
