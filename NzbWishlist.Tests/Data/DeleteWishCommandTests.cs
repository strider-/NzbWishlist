using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(DeleteWishCommand))]
    public class DeleteWishCommandTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Deletes_Results_For_The_Wish_And_The_Wish_Itself()
        {
            var w = new Wish { RowKey = "123" };
            var wishResultId = "456";
            var queryResult = new List<DynamicTableEntity>
            {
                new DynamicTableEntity(nameof(WishResult), $"{w.RowKey}_{wishResultId}", "*", new Dictionary<string, EntityProperty>())
            };

            _table.SetupSegmentedQuery(queryResult);
            _table.SetupBatch();
            _table.SetupOperation(w, TableOperationType.Delete);
            var cmd = new DeleteWishCommand(w.RowKey);

            await cmd.ExecuteAsync(_table.Object);

            _table.VerifyBatch();
            _table.VerifyOperation(w, TableOperationType.Delete);
            _table.VerifySegmentedQuery<DynamicTableEntity>();
        }
    }
}
