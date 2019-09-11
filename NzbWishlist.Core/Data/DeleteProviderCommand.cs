using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public class DeleteProviderCommand : ICommandAsync<CloudTable>
    {
        private readonly string _providerId;

        public DeleteProviderCommand(string providerId) => _providerId = providerId;

        public async Task ExecuteAsync(CloudTable model)
        {
            var op = TableOperation.Delete(new Provider { RowKey = _providerId, ETag = "*" });

            await model.ExecuteAsync(op);
        }
    }
}
