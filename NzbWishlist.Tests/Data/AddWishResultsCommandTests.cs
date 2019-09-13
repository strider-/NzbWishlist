using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(AddWishResultsCommand))]
    public class AddWishResultsCommandTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Does_Nothing_With_No_Results()
        {
            var results = Enumerable.Empty<WishResult>();
            var cmd = new AddWishResultsCommand(results);

            await cmd.ExecuteAsync(_table.Object);

            _table.Verify(t => t.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()), Times.Never());
        }

        [Fact]
        public async Task ExecuteAsync_Throws_An_Exception_For_Any_Result_Not_Assigned_To_A_Wish()
        {
            var results = new[] { new WishResult() };
            var cmd = new AddWishResultsCommand(results);

            var ex = await Record.ExceptionAsync(() => cmd.ExecuteAsync(_table.Object));

            Assert.NotNull(ex);
            var aex = Assert.IsType<ApplicationException>(ex);
            Assert.Equal("One or more wish results haven't been assigned to a wish!", aex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_Persists_Results_In_Batches()
        {
            _table.SetupBatch();
            var w = new Wish() { Name = "I Wish" };
            var wr = new WishResult();
            wr.BelongsTo(w);
            var results = new[] { wr};
            var cmd = new AddWishResultsCommand(results);

            await cmd.ExecuteAsync(_table.Object);

            _table.VerifyBatch();
        }
    }
}
