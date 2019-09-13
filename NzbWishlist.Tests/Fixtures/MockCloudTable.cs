using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using System;

namespace NzbWishlist.Tests.Fixtures
{
    public class MockCloudTable : Mock<CloudTable>
    {
        public MockCloudTable() : base(MockBehavior.Strict, new[] { new Uri("https://no.where/devstoreaccount1/") })
        {

        }

        public void SetupOperation(ITableEntity entity, TableOperationType operation)
        {
            Setup(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == operation && op.Entity.RowKey == entity.RowKey)))
                .ReturnsAsync(new TableResult
                {
                    Etag = "new!",
                    HttpStatusCode = 200,
                    Result = entity
                });
        }

        public void VerifyOperation(ITableEntity entity, TableOperationType operation)
        {
            Verify(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == operation && op.Entity.RowKey == entity.RowKey)), Times.Once());
        }
    }
}
