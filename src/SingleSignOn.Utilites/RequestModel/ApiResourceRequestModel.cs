using System.Collections.Generic;

namespace SingleSignOn.Utilites.RequestModel
{
    public class ApiResourceRequestModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool ShowInDiscoveryDocument { get; set; }
        public string AllowedAccessTokenSigningAlgorithms { get; set; }
        public List<string> UserClaims { get; set; }
        public List<string> Scopes { get; set; }
    }
}
