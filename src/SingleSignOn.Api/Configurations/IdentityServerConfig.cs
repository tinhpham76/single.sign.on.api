using System.Collections.Generic;
using IdentityServer4.Models;

namespace SingleSignOn.Api.Configurations
{
    public class IdentityServerConfig
    {
        public const string ConfigName = "IdentityServer";

        public List<Client> Clients { get; set; }
        public List<ApiResource> ApiResources { get; set; }
        public List<ApiScope> ApiScopes { get; set; }
        public List<IdentityResource> IdentityResources {get; set;}
    }
}