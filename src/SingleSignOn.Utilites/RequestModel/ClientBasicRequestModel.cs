using System.Collections.Generic;

namespace SingleSignOn.Utilites.RequestModel
{
    public class ClientBasicRequestModel
    {
        public string ClientName { get; set; }
        public string Description { get; set; }
        public string ClientUri { get; set; }
        public string LogoUri { get; set; }
        public List<string> AllowedCorsOrigins { get; set; }
    }
}
