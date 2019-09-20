using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public class GetCartEntryQuery : IQueryAsync<CloudTable, CartEntry>
    {
        private readonly string _cartId;

        public GetCartEntryQuery(string cartId) => _cartId = cartId;

        public async Task<CartEntry> ExecuteAsync(CloudTable model)
        {
            var op = TableOperation.Retrieve<CartEntry>(nameof(CartEntry), _cartId);

            var result = await model.ExecuteAsync(op);

            return (CartEntry)result.Result;
        }
    }
}
