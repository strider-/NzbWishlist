using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(GetWishResultQuery))]
    public class GetWishResultQueryTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Returns_The_Specified_Wish_Result()
        {
            var wr = new WishResult { RowKey = "000_123" };
            _table.SetupOperation(TableOperationType.Retrieve, () => wr);

            var query = new GetWishResultQuery(wr.RowKey);
            var result = await query.ExecuteAsync(_table.Object);

            _table.VerifyOperation(TableOperationType.Retrieve);
            Assert.Equal(wr.RowKey, result.RowKey);
        }
    }
}
