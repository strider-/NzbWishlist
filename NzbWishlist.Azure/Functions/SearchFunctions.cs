using Microsoft.Azure.WebJobs;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Functions
{
    public class SearchFunctions
    {
        private readonly INewznabClient _client;

        public SearchFunctions(INewznabClient client)
        {
            _client = client;
        }

        [FunctionName(Constants.WishSearchActivity)]
        public async Task<IEnumerable<WishResult>> WishSearchAsync([ActivityTrigger] SearchWishContext context)
        {
            var results = await _client.SearchAsync(context.Provider, context.Wish);

            foreach(var result in results)
            {
                result.BelongsTo(context.Wish);
            }

            return results;
        }
    }
}