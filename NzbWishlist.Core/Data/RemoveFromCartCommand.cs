using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public class RemoveFromCartCommand : ICommandAsync<CloudTable>
    {
        public readonly CartEntry _entry;

        public RemoveFromCartCommand(CartEntry entry) => _entry = entry;

        public async Task ExecuteAsync(CloudTable model)
        {
            var op = TableOperation.Delete(_entry);

            await model.ExecuteAsync(op);
        }
    }
}