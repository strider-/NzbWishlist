using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public class UpdateLastSearchDateCommand : ICommandAsync<CloudTable>
    {
        private readonly IEnumerable<Wish> _wishes;

        public UpdateLastSearchDateCommand(IEnumerable<Wish> wishes) => _wishes = wishes;

        public async Task ExecuteAsync(CloudTable model)
        {
            var newDate = DateTime.UtcNow;
            var currentBatch = _wishes.Take(100);
            int counter = 1;

            do
            {
                var batch = new TableBatchOperation();
                foreach (var wish in currentBatch)
                {
                    wish.LastSearchDate = newDate;
                    batch.InsertOrMerge(wish);
                }

                await model.ExecuteBatchAsync(batch);

                currentBatch = _wishes.Skip(100 * counter++).Take(100);
            } while (currentBatch.Any());
        }
    }
}