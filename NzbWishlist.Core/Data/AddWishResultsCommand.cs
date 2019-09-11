using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public class AddWishResultsCommand : ICommandAsync<CloudTable>
    {
        private readonly IEnumerable<WishResult> _results;

        public AddWishResultsCommand(IEnumerable<WishResult> results) => _results = results;

        public async Task ExecuteAsync(CloudTable model)
        {
            if (_results.Any(r => r.RowKey == null))
            {
                throw new ArgumentException("One or more wish results haven't been assigned to a wish!");
            }

            var batch = new TableBatchOperation();

            foreach (var result in _results)
            {
                batch.Insert(result);
            }

            await model.ExecuteBatchAsync(batch);
        }
    }
}