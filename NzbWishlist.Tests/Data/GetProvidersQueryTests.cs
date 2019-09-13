using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(GetProvidersQuery))]
    public class GetProvidersQueryTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Returns_All_Providers()
        {
            var providers = new List<Provider>
            {
                new Provider(),
                new Provider()
            };
            _table.SetupSegmentedQuery(providers);

            var cmd = new GetProvidersQuery();
            var results = await cmd.ExecuteAsync(_table.Object);

            _table.VerifySegmentedQuery<Provider>();
            Assert.Equal(2, results.Count());
        }
    }
}