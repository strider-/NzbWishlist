using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Extensions;
using NzbWishlist.Azure.Framework;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Functions
{
    public class OrchestrationFunctions
    {
        const string MainOrchestrator = "SearchOrchestration";
        const string SubOrchestrator = "ProviderOrchestration";

        [FunctionName("SearchTrigger")]
        public async Task SearchAsync(
            [TimerTrigger("0 0 */2 * * *", RunOnStartup = true)] TimerInfo timer,
            [Table(Constants.ProviderTableName)] CloudTable providerTable,
            [Table(Constants.WishTableName)] CloudTable wishTable,
            [OrchestrationClient] DurableOrchestrationClientBase client)
        {
            var providers = await providerTable.ExecuteAsync(new GetProvidersQuery());
            var wishes = await wishTable.ExecuteAsync(new GetWishesQuery());
            var activeWishes = wishes.Where(w => w.Active);

            if (activeWishes.Any())
            {
                await client.StartNewAsync(MainOrchestrator, new SearchContext
                {
                    Providers = providers,
                    Wishes = activeWishes
                });
            }
        }

        [FunctionName(MainOrchestrator)]
        public async Task SearchOrchestrationAsync(
            [OrchestrationTrigger] DurableOrchestrationContextBase context,
            [Table(Constants.WishTableName)] CloudTable wishTable,
            [Pushover] IAsyncCollector<PushoverNotification> notifications)
        {
            var model = context.GetInput<SearchContext>();
            var results = new List<WishResult>();

            foreach (var provider in model.Providers)
            {
                var providerResults = await context.CallSubOrchestratorAsync<IEnumerable<WishResult>>(SubOrchestrator, new SearchProviderContext
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

                var wishNames = results.GroupBy(r => r.WishName)
                                       .Select(r => r.Key)
                                       .Distinct();
                
                await notifications.AddAsync(new PushoverNotification
                {
                    Title = "NZB Wishlist",
                    Message = $"Found new results for {string.Join(", ", wishNames)}"
                });
            }

            var updateWishesCmd = new UpdateLastSearchDateCommand(model.Wishes);
            await wishTable.ExecuteAsync(updateWishesCmd);
        }

        [FunctionName(SubOrchestrator)]
        public async Task<IEnumerable<WishResult>> ProviderOrchestrationAsync([OrchestrationTrigger] DurableOrchestrationContextBase context)
        {
            var model = context.GetInput<SearchProviderContext>();
            var results = new List<WishResult>();

            foreach (var wish in model.Wishes)
            {
                var wishResults = await context.CallActivityAsync<IEnumerable<WishResult>>(Constants.WishSearchActivity, new SearchWishContext
                {
                    Provider = model.Provider,
                    Wish = wish
                });
                results.AddRange(wishResults);
            }

            return results;
        }

        [FunctionName("PurgeSearchHistory")]
        public async Task PurgeSearchHistoryAsync(
            [TimerTrigger("0 0 0 * * *")] TimerInfo timer,
            [OrchestrationClient] DurableOrchestrationClientBase client)
        {
            // delete all non-running batch calls older than a week at midnight UTC every day
            await client.PurgeInstanceHistoryAsync(
                DateTime.MinValue,
                DateTime.UtcNow.AddDays(-7),
                new OrchestrationStatus[] {
                    OrchestrationStatus.Completed,
                    OrchestrationStatus.Canceled,
                    OrchestrationStatus.Failed,
                    OrchestrationStatus.Terminated
            });
        }
    }
}