using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NzbWishlist.Azure;
using NzbWishlist.Core.Services;

[assembly: FunctionsStartup(typeof(Startup))]

namespace NzbWishlist.Azure
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            builder.Services.AddScoped<INewznabClient, NewznabClient>();
        }
    }
}