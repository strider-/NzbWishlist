using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(ToggleWishCommand))]
    public class ToggleWishCommandTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Throws_An_Exception_If_The_Wish_Doesnt_Exist()
        {
            _table.SetupOperationToFail(TableOperationType.Retrieve);

            var cmd = new ToggleWishCommand("123", true);
            var ex = await Record.ExceptionAsync(() => cmd.ExecuteAsync(_table.Object));

            _table.VerifyFailedOperation(TableOperationType.Retrieve);
            Assert.NotNull(ex);
            Assert.Equal("Wish id 123 does not exist.", ex.Message);
        }

        [Fact]
        public async Task ExecuteAsync_Updates_The_Active_Property()
        {
            var wish = new Wish { RowKey = "123", Active = false };
            _table.SetupSequence(t => t.ExecuteAsync(It.IsAny<TableOperation>()))
                .ReturnsAsync(new TableResult
                {
                    Etag = "new",
                    HttpStatusCode = 200,
                    Result = new DynamicTableEntity(wish.PartitionKey, wish.RowKey, "new", new Dictionary<string, EntityProperty> {
                        { "Active", new EntityProperty(false) }
                    })
                })
                .ReturnsAsync(new TableResult
                {
                    Etag = "new",
                    HttpStatusCode = 200,
                    Result = new DynamicTableEntity(wish.PartitionKey, wish.RowKey, "new", new Dictionary<string, EntityProperty> {
                        { "Active", new EntityProperty(true) }
                    })
                });

            var cmd = new ToggleWishCommand(wish.RowKey, true);
            await cmd.ExecuteAsync(_table.Object);

            _table.Verify(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == TableOperationType.Retrieve)), Times.Once());
            _table.Verify(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == TableOperationType.Merge)), Times.Once());
        }
    }
}
