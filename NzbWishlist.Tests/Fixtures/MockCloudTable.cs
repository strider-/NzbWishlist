using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NzbWishlist.Tests.Fixtures
{
    public class MockCloudTable : Mock<CloudTable>
    {
        public MockCloudTable() 
            : base(MockBehavior.Strict, new[] { new Uri("https://no.where/devstoreaccount1/") })
        { }

        public void SetupOperation<T>(TableOperationType operation, Func<T> creator = null) where T : ITableEntity, new()
        {
            Setup(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == operation)))
                .ReturnsAsync(new TableResult
                {
                    Etag = "new!",
                    HttpStatusCode = 200,
                    Result = creator == null ? new T() : creator.Invoke()
                });
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

        public void SetupOperationToFail(TableOperationType operation)
        {
            Setup(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == operation)))
                .ReturnsAsync(new TableResult
                {
                    HttpStatusCode = 404,
                    Result = null
                });
        }

        public void SetupOperationToThrow(TableOperationType? type = null)
        {
            if (type == null)
            {
                Setup(t => t.ExecuteAsync(It.IsAny<TableOperation>())).ThrowsAsync(new Exception("uh oh"));
            }
            else
            {
                Setup(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == type.Value))).ThrowsAsync(new Exception("uh oh"));
            }
        }

        public void VerifyOperation(TableOperationType operation)
        {
            Verify(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == operation)), Times.Once());
        }

        public void VerifyOperation(ITableEntity entity, TableOperationType operation)
        {
            Verify(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == operation && op.Entity.RowKey == entity.RowKey)), Times.Once());
        }

        public void VerifyFailedOperation(TableOperationType operation)
        {
            Verify(t => t.ExecuteAsync(It.Is<TableOperation>(op => op.OperationType == operation)), Times.Once());
        }

        public void SetupBatch()
        {
            Setup(t => t.ExecuteBatchAsync(It.IsAny<TableBatchOperation>())).ReturnsAsync(new List<TableResult>());
        }

        public void VerifyBatch()
        {
            Verify(t => t.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()), Times.AtLeastOnce());
        }

        public void SetupSegmentedQueryToThrow()
        {
            Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery>(), It.IsAny<TableContinuationToken>())).ThrowsAsync(new Exception("uh oh"));
        }

        public void SetupSegmentedQuery<T>(IEnumerable<T> queryReturnValue) where T : ITableEntity, new()
        {
            var segment = CreateTableQuerySegment(queryReturnValue);

            Setup(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<T>>(), It.IsAny<TableContinuationToken>())).ReturnsAsync(segment);
        }

        public void VerifySegmentedQuery<T>() where T : ITableEntity, new()
        {
            Verify(t => t.ExecuteQuerySegmentedAsync(It.IsAny<TableQuery<T>>(), It.IsAny<TableContinuationToken>()), Times.AtLeastOnce());
        }

        public TableQuerySegment<T> CreateTableQuerySegment<T>(IEnumerable<T> col)
        {
            var ctor = typeof(TableQuerySegment<T>)
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Count() == 1);

            return ctor.Invoke(new[] { col.ToList() }) as TableQuerySegment<T>;
        }
    }
}
