using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using NzbWishlist.Azure;

[assembly: FunctionsStartup(typeof(Startup))]

namespace NzbWishlist.Azure
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            
        }
    }
}
