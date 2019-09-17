using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NzbWishlist.Azure;
using NzbWishlist.Azure.Framework;
using NzbWishlist.Core.Services;
using System.Net.Http;
using System.Runtime.CompilerServices;

[assembly: FunctionsStartup(typeof(Startup))]
[assembly: WebJobsStartup(typeof(WebJobsStartup))]
[assembly: InternalsVisibleTo("NzbWishlist.Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace NzbWishlist.Azure
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<INewznabClient, NewznabClient>()
                            .AddSingleton<IHttpClientFactory, DynamicHttpClientFactory>();
        }
    }

    public class WebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.AddExtension<PushoverExtensions>();
        }
    }
}