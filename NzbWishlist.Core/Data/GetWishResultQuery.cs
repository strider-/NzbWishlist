using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public class GetWishResultQuery : IQueryAsync<CloudTable, WishResult>
    {
        private readonly string _wishResultId;

        public GetWishResultQuery(string wishResultId) => _wishResultId = wishResultId;

        public async Task<WishResult> ExecuteAsync(CloudTable model)
        {
            var op = TableOperation.Retrieve<WishResult>(nameof(WishResult), _wishResultId);

            var result = await model.ExecuteAsync(op);

            return (WishResult)result.Result;
        }
    }
}
