using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    public class GetWishesQueryTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Returns_All_Wishes()
        {
            var providers = new List<Wish>
            {
                new Wish(),
                new Wish(),
                new Wish()
            };
            _table.SetupSegmentedQuery(providers);

            var cmd = new GetWishesQuery();
            var results = await cmd.ExecuteAsync(_table.Object);

            _table.VerifySegmentedQuery<Wish>();
            Assert.Equal(3, results.Count());
        }
    }
}
