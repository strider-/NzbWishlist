using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    public class GetWishResultsQueryTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Returns_All_Results_For_A_Wish()
        {
            var providers = new List<WishResult>
            {
                new WishResult(),
                new WishResult(),
                new WishResult()
            };
            _table.SetupSegmentedQuery(providers);

            var cmd = new GetWishResultsQuery("123");
            var results = await cmd.ExecuteAsync(_table.Object);

            _table.VerifySegmentedQuery<WishResult>();
            Assert.Equal(3, results.Count());
        }
    }
}
