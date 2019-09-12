using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Core.Services;
using System;
using System.Collections.Generic;
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
            [TimerTrigger("0 0 */2 * * *", RunOnStartup = true)] TimerInfo timer,
            [Table(Constants.ProviderTableName)] CloudTable providerTable,
            [Table(Constants.WishTableName)] CloudTable wishTable,
            [OrchestrationClient] DurableOrchestrationClientBase client)
        {
            var providers = await new GetProvidersQuery().ExecuteAsync(providerTable);
            var wishes = await new GetWishesQuery().ExecuteAsync(wishTable);

            await client.StartNewAsync("SearchOrchestration", new SearchContext
            {
                Providers = providers,
                Wishes = wishes
            });
        }

        [FunctionName("SearchOrchestration")]
        public async Task SearchOrchestrationAsync(
            [OrchestrationTrigger] DurableOrchestrationContextBase context,
            [Table(Constants.WishTableName)] CloudTable wishTable)
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

            var command = new AddWishResultsCommand(results);

            await command.ExecuteAsync(wishTable);
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