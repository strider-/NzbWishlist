using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(AddToCartCommand))]
    public class AddToCartCommandTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Persists_A_New_CartEntry()
        {
            var e = new CartEntry();
            _table.SetupOperation(e, TableOperationType.Insert);
            var cmd = new AddToCartCommand(e);

            await cmd.ExecuteAsync(_table.Object);

            _table.VerifyOperation(e, TableOperationType.Insert);
        }
    }
}