using Microsoft.AspNetCore.Hosting;

[assembly: HostingStartup(typeof(SingleSignOn.Api.Areas.Identity.IdentityHostingStartup))]
namespace SingleSignOn.Api.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}