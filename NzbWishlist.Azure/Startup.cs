using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NzbWishlist.Azure;
using NzbWishlist.Azure.Framework;
using NzbWishlist.Core.Services;
using System.Net.Http;

[assembly: FunctionsStartup(typeof(Startup))]
[assembly: WebJobsStartup(typeof(WebJobsStartup))]

namespace NzbWishlist.Azure
{
    public class Startup : FunctionsStartup, IWebJobsStartup
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