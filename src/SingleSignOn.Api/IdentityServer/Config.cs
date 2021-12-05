using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace SingleSignOn.Api.IdentityServer
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResources.Phone(),
                new IdentityResources.Address()
            };
        }
        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("SSO_SERVER", "SSO Server API Resources")
                {
                    ApiSecrets = { new Secret("secret".Sha256()) },
                    Scopes = { "SSO_SERVER" }
                }
            };
        }
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                // backward compat
                new ApiScope("SSO_SERVER"),
            };
        }
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client> {
                new Client
                {
                    ClientId = "swagger_sso_server",
                    ClientName = "Swagger SSO Server",
                    LogoUri = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/ee/.NET_Core_Logo.svg/512px-.NET_Core_Logo.svg.png",

                    AllowedGrantTypes = GrantTypes.Implicit,
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false,

                    RedirectUris =           { "https://sso.core.api.xxx98qn.xyz/swagger/oauth2-redirect.html" },
                    PostLogoutRedirectUris = { "https://sso.core.api.xxx98qn.xyz/swagger" },
                    AllowedCorsOrigins =     { "https://sso.core.api.xxx98qn.xyz" },

                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "SSO_SERVER"
                    }
                },
                new Client
                {
                    ClientName = "Angular Admin Dashboard",
                    ClientId = "angular_admin_dashboard",
                    LogoUri = "https://angular.io/assets/images/logos/angular/angular.svg",
                    AccessTokenType = AccessTokenType.Reference,
                    RequireConsent = false,

                    RequireClientSecret = false,
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = new List<string>
                    {
                        "http://localhost:4200/auth-callback",
                        "http://localhost:4200/silent-renew.html"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:4200/"
                    },
                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:4200"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "SSO_SERVER",
                    }
                },
                new Client
                {
                    ClientName = "Angular User Profile",
                    ClientId = "angular_user_profile",
                    LogoUri = "https://angular.io/assets/images/logos/angular/angular.svg",
                    AccessTokenType = AccessTokenType.Reference,
                    RequireConsent = false,

                    RequireClientSecret = false,
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,

                    AllowAccessTokensViaBrowser = true,
                    RedirectUris = new List<string>
                    {
                        "http://localhost:4300/auth-callback",
                        "http://localhost:4300/silent-renew.html"
                    },
                    PostLogoutRedirectUris = new List<string>
                    {
                        "http://localhost:4300/"
                    },
                    AllowedCorsOrigins = new List<string>
                    {
                        "http://localhost:4300"
                    },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "SSO_SERVER",
                    }
                }
            };
        }
    }
}
