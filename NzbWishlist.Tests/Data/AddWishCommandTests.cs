using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(AddWishCommand))]
    public class AddWishCommandTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Persists_A_New_Wish()
        {
            var w = new Wish();
            var cmd = new AddWishCommand(w);
            _table.SetupOperation(w, TableOperationType.Insert);

            await cmd.ExecuteAsync(_table.Object);

            _table.VerifyOperation(w, TableOperationType.Insert);
        }
    }
}
