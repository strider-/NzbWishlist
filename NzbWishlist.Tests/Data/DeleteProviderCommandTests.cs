using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(DeleteProviderCommand))]
    public class DeleteProviderCommandTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Deletes_The_Given_Provider()
        {
            var id = "123456";
            var provider = new Provider { RowKey = id };
            _table.SetupOperation(provider, TableOperationType.Delete);

            var cmd = new DeleteProviderCommand(id);

            await cmd.ExecuteAsync(_table.Object);

            _table.VerifyOperation(provider, TableOperationType.Delete);
        }
    }
}