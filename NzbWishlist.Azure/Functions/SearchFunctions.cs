using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public async Task<IEnumerable<WishResult>> WishSearchAsync(
            [ActivityTrigger] SearchWishContext context,
            ILogger log)
        {
            try
            {
                var results = await _client.SearchAsync(context.Provider, context.Wish);

                foreach (var result in results)
                {
                    result.BelongsTo(context.Wish);
                }

                return results;
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "NewznabClient caused an exception");
                return Enumerable.Empty<WishResult>();
            }
        }
    }
}