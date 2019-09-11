using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public class DeleteWishCommand : ICommandAsync<CloudTable>
    {
        const int MaxBatchSize = 100;

        private readonly string _wishRowKey;

        public DeleteWishCommand(string wishRowKey) => _wishRowKey = wishRowKey;

        public async Task ExecuteAsync(CloudTable model)
        {
            var filter = new RowKeyStartsWithFilter(nameof(WishResult), _wishRowKey).ToString();
            var query = new TableQuery<DynamicTableEntity>().Where(filter).Select(new[] { "RowKey" });
            TableContinuationToken token = null;

            query.TakeCount = MaxBatchSize;

            do
            {
                var batch = new TableBatchOperation();
                var items = await model.ExecuteQuerySegmentedAsync(query, token);
                token = items.ContinuationToken;

                foreach (var item in items)
                {
                    batch.Delete(item);
                }

                if (batch.Any())
                {
                    await model.ExecuteBatchAsync(batch);
                }
            } while (token != null);

            var op = TableOperation.Delete(new Wish { RowKey = _wishRowKey, ETag = "*" });
            await model.ExecuteAsync(op);
        }
    }
}
