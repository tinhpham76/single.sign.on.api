using System.Collections.Generic;

namespace SingleSignOn.Utilites.ViewModel
{
    public class ClientBasicViewModel
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string Description { get; set; }
        public string ClientUri { get; set; }
        public string LogoUri { get; set; }
        public List<string> AllowedCorsOrigins { get; set; }
    }
}
