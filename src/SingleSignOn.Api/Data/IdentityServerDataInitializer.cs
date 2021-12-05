
using IdentityServer4.Models;
using SingleSignOn.Api.Configurations;
using System.Collections.Generic;

namespace SingleSignOn.Api.Data
{
    public class IdentityServerDataInitializer
    {
        private readonly IdentityServerConfig _identtiyServerConfig;
        public IdentityServerDataInitializer(IdentityServerConfig identityServerConfig)
        {
            _identtiyServerConfig = identityServerConfig;
        }

        public IEnumerable<IdentityResource> GetIdentityResources() => _identtiyServerConfig.IdentityResources;
        public IEnumerable<ApiResource> GetApiResources() => _identtiyServerConfig.ApiResources;
        public IEnumerable<ApiScope> GetApiScopes() => _identtiyServerConfig.ApiScopes;
        public IEnumerable<Client> GetClients() =>  _identtiyServerConfig.Clients;
    }
}
