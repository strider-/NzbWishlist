using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Extensions;
using NzbWishlist.Azure.Framework;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Functions
{
    public class SearchFunctions
    {
        private readonly INewznabClient _client;

        public SearchFunctions(INewznabClient client) => _client = client;

        [FunctionName("SearchTrigger")]
        public async Task SearchAsync(
            [TimerTrigger("0 0 */2 * * *")] TimerInfo timer,
            [Table(Constants.ProviderTableName)] CloudTable providerTable,
            [Table(Constants.WishTableName)] CloudTable wishTable,
            [OrchestrationClient] DurableOrchestrationClientBase client)
        {
            var providers = await providerTable.ExecuteAsync(new GetProvidersQuery());
            var wishes = await wishTable.ExecuteAsync(new GetWishesQuery());

            await client.StartNewAsync("SearchOrchestration", new SearchContext
            {
                Providers = providers,
                Wishes = wishes.Where(w => w.Active)
            });
        }

        [FunctionName("SearchOrchestration")]
        public async Task SearchOrchestrationAsync(
            [OrchestrationTrigger] DurableOrchestrationContextBase context,
            [Table(Constants.WishTableName)] CloudTable wishTable,
            [Pushover] IAsyncCollector<PushoverNotification> notifications)
        {
            var model = context.GetInput<SearchContext>();
            var results = new List<WishResult>();

            foreach (var provider in model.Providers)
            {
                var providerResults = await context.CallSubOrchestratorAsync<IEnumerable<WishResult>>("ProviderOrchestration", new SearchProviderContext
                {
                    Provider = provider,
                    Wishes = model.Wishes
                });

                results.AddRange(providerResults);
            }

            if (results.Any())
            {
                var addResultsCmd = new AddWishResultsCommand(results);
                await wishTable.ExecuteAsync(addResultsCmd);

                var wishNames = results.GroupBy(r => r.WishName).Distinct();
                await notifications.AddAsync(new PushoverNotification
                {
                    Title = "NZB Wishlist",
                    Message = $"Found new results for {string.Join(", ", wishNames)}"
                });
            }

            var updateWishesCmd = new UpdateLastSearchDateCommand(model.Wishes);
            await wishTable.ExecuteAsync(updateWishesCmd);
        }

        [FunctionName("ProviderOrchestration")]
        public async Task<IEnumerable<WishResult>> ProviderOrchestrationAsync([OrchestrationTrigger] DurableOrchestrationContextBase context)
        {
            var model = context.GetInput<SearchProviderContext>();
            var results = new List<WishResult>();

            foreach (var wish in model.Wishes)
            {
                var wishResults = await context.CallActivityAsync<IEnumerable<WishResult>>("WishSearch", new SearchWishContext
                {
                    Provider = model.Provider,
                    Wish = wish
                });
                results.AddRange(wishResults);

                await context.CreateTimer(DateTime.UtcNow.AddSeconds(1), CancellationToken.None);
            }

            return results;
        }

        [FunctionName("WishSearch")]
        public async Task<IEnumerable<WishResult>> WishSearchAsync([ActivityTrigger] SearchWishContext context)
        {
            return await _client.SearchAsync(context.Provider, context.Wish);
        }
    }
}