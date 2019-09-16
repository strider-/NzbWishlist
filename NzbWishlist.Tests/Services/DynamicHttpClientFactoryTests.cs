using NzbWishlist.Core.Services;
using Xunit;

namespace NzbWishlist.Tests.Services
{
    [Trait(nameof(Services), nameof(DynamicHttpClientFactory))]
    public class DynamicHttpClientFactoryTests
    {
        public readonly DynamicHttpClientFactory _factory = new DynamicHttpClientFactory();

        [Fact]
        public void CreateClient_Generates_New_Clients_Per_Name()
        {
            var clientA = _factory.CreateClient("A");
            var clientB = _factory.CreateClient("B");

            Assert.NotEqual(clientA, clientB);
        }

        [Fact]
        public void CreateClient_Returns_The_Same_Client_Per_Name()
        {
            var clientA = _factory.CreateClient("A");
            var clientB = _factory.CreateClient("A");

            Assert.Equal(clientA, clientB);
        }
    }
}