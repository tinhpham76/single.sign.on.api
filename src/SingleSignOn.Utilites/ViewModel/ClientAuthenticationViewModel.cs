using System.Collections.Generic;

namespace SingleSignOn.Utilites.ViewModel
{
    public class ClientAuthenticationViewModel
    {
        public bool EnableLocalLogin { get; set; }
        public List<string> PostLogoutRedirectUris { get; set; }
        public string FrontChannelLogoutUri { get; set; }
        public bool FrontChannelLogoutSessionRequired { get; set; }
        public string BackChannelLogoutUri { get; set; }
        public bool BackChannelLogoutSessionRequired { get; set; }
        public int? UserSsoLifetime { get; set; }
    }
}
