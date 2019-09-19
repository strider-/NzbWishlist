using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(GetCartQuery))]
    public class GetCartQueryTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Returns_All_Cart_Entries()
        {
            var entries = new List<CartEntry>
            {
                new CartEntry(),
                new CartEntry()
            };
            _table.SetupSegmentedQuery(entries);

            var cmd = new GetCartQuery();
            var results = await cmd.ExecuteAsync(_table.Object);

            _table.VerifySegmentedQuery<CartEntry>();
            Assert.Equal(2, results.Count());
        }
    }
}
