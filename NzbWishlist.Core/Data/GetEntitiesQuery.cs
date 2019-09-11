using Microsoft.WindowsAzure.Storage.Table;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public abstract class GetEntitiesQuery<T> : IQueryAsync<CloudTable, IEnumerable<T>> where T : ITableEntity, new()
    {
        public GetEntitiesQuery(string partitionKey) => PartitionKeyFilter = partitionKey;

        public virtual async Task<IEnumerable<T>> ExecuteAsync(CloudTable model)
        {
            var list = new List<T>();
            TableContinuationToken token = null;

            var query = Query();

            do
            {
                var result = await model.ExecuteQuerySegmentedAsync(query, token);
                token = result.ContinuationToken;
                list.AddRange(result);
            } while (token != null);

            return list;
        }

        protected virtual TableQuery<T> Query()
        {
            return new TableQuery<T>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, PartitionKeyFilter)
            );
        }

        protected string PartitionKeyFilter { get; }
    }
}
