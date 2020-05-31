using DurableTask.Core;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Timers;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using NzbWishlist.Azure.Framework;
using NzbWishlist.Azure.Functions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Functions
{
    [Trait(nameof(Functions), nameof(OrchestrationFunctions))]
    public class OrchestrationFunctionsTests
    {
        private readonly OrchestrationFunctions _function = new OrchestrationFunctions();
        private readonly MockCloudTable _wishTable = new MockCloudTable();

        [Fact]
        public async Task SearchAsync_Does_Nothing_When_There_Are_No_Active_Wishes()
        {
            var client = new Mock<DurableOrchestrationClientBase>(MockBehavior.Strict);
            var timer = new TimerInfo(new TimerScheduleStub(), new ScheduleStatus());
            var providerTable = new MockCloudTable();
            providerTable.SetupSegmentedQuery(Enumerable.Empty<Provider>());
            _wishTable.SetupSegmentedQuery(new[]
            {
                new Wish { Active = false }
            });

            await _function.SearchAsync(timer, providerTable.Object, _wishTable.Object, client.Object);

            providerTable.VerifySegmentedQuery<Provider>();
            _wishTable.VerifySegmentedQuery<Wish>();
            client.Verify(c => c.StartNewAsync("SearchOrchestration", It.IsAny<object>()), Times.Never());
        }

        [Fact]
        public async Task SearchAsync_Starts_The_Search_Orchestration()
        {
            var client = new Mock<DurableOrchestrationClientBase>(MockBehavior.Strict);
            var timer = new TimerInfo(new TimerScheduleStub(), new ScheduleStatus());
            var providerTable = new MockCloudTable();
            providerTable.SetupSegmentedQuery(new[]
            {
                new Provider()
            });
            _wishTable.SetupSegmentedQuery(new[]
            {
                new Wish { Active = true }
            });
            client.Setup(c => c.StartNewAsync("SearchOrchestration", It.IsAny<object>())).ReturnsAsync("newid");

            await _function.SearchAsync(timer, providerTable.Object, _wishTable.Object, client.Object);

            providerTable.VerifySegmentedQuery<Provider>();
            _wishTable.VerifySegmentedQuery<Wish>();
            client.Verify(c => c.StartNewAsync("SearchOrchestration", It.IsAny<object>()), Times.Once());
        }

        [Fact]
        public async Task SearchOrchestrationAsync_Does_Nothing_When_There_Are_No_Providers()
        {
            var searchContext = new SearchContext
            {
                Providers = Enumerable.Empty<Provider>(),
                Wishes = Enumerable.Empty<Wish>()
            };
            var context = new Mock<DurableOrchestrationContextBase>(MockBehavior.Strict);
            var notifications = new Mock<IAsyncCollector<PushoverNotification>>(MockBehavior.Strict);
            context.Setup(c => c.GetInput<SearchContext>()).Returns(searchContext);

            await _function.SearchOrchestrationAsync(context.Object, _wishTable.Object, notifications.Object);

            context.Verify(c => c.GetInput<SearchContext>(), Times.Once());
            context.Verify(c => c.CallSubOrchestratorAsync<IEnumerable<WishResult>>("ProviderOrchestration", It.IsAny<object>()), Times.Never());
            notifications.Verify(n => n.AddAsync(It.IsAny<PushoverNotification>(), CancellationToken.None), Times.Never());
            _wishTable.Verify(t => t.ExecuteBatchAsync(It.IsAny<TableBatchOperation>()), Times.Never());
        }

        [Fact]
        public async Task SearchOrchestrationAsync_Starts_The_Provider_Sub_Orchestration_And_Sets_Wish_LastSearchDate()
        {
            var providers = new[]
            {
                new Provider { ApiKey = "key", ApiUrl = "https://no.where" },
                new Provider { ApiKey = "key2", ApiUrl = "https://no2.where" },
            };
            var searchContext = new SearchContext
            {
                Providers = providers,
                Wishes = new[] { new Wish { Name = "wish", Query = "query", LastSearchDate = DateTime.UtcNow.AddDays(-2) } }
            };
            var context = new Mock<DurableOrchestrationContextBase>(MockBehavior.Strict);
            var notifications = new Mock<IAsyncCollector<PushoverNotification>>(MockBehavior.Strict);
            context.Setup(c => c.GetInput<SearchContext>()).Returns(searchContext);
            context.Setup(c => c.CallSubOrchestratorAsync<IEnumerable<WishResult>>("ProviderOrchestration", It.IsAny<object>()))
                   .ReturnsAsync(Enumerable.Empty<WishResult>());
            _wishTable.SetupBatch();

            await _function.SearchOrchestrationAsync(context.Object, _wishTable.Object, notifications.Object);

            context.Verify(c => c.GetInput<SearchContext>(), Times.Once());
            context.Verify(c => c.CallSubOrchestratorAsync<IEnumerable<WishResult>>("ProviderOrchestration", It.IsAny<object>()), Times.Exactly(2));
            notifications.Verify(n => n.AddAsync(It.IsAny<PushoverNotification>(), CancellationToken.None), Times.Never());
            _wishTable.VerifyBatch();
        }

        [Fact]
        public async Task SearchOrchestrationAsync_Persists_WishResults_And_Sends_A_Notification()
        {
            var wish = new Wish { Name = "wish", Query = "query", LastSearchDate = DateTime.UtcNow.AddDays(-2) };
            var wishResult = new WishResult();
            wishResult.BelongsTo(wish);
            var searchContext = new SearchContext
            {
                Providers = new[] { new Provider { ApiKey = "key", ApiUrl = "https://no.where" } },
                Wishes = new[] { wish }
            };
            var context = new Mock<DurableOrchestrationContextBase>(MockBehavior.Strict);
            var notifications = new Mock<IAsyncCollector<PushoverNotification>>(MockBehavior.Strict);
            context.Setup(c => c.GetInput<SearchContext>()).Returns(searchContext);
            context.Setup(c => c.CallSubOrchestratorAsync<IEnumerable<WishResult>>("ProviderOrchestration", It.IsAny<object>()))
                   .ReturnsAsync(new[] { wishResult });
            notifications.Setup(n => n.AddAsync(It.IsAny<PushoverNotification>(), CancellationToken.None))
                         .Returns(Task.CompletedTask);
            _wishTable.SetupBatch();

            await _function.SearchOrchestrationAsync(context.Object, _wishTable.Object, notifications.Object);

            context.Verify(c => c.GetInput<SearchContext>(), Times.Once());
            context.Verify(c => c.CallSubOrchestratorAsync<IEnumerable<WishResult>>("ProviderOrchestration", It.IsAny<object>()), Times.Once());
            notifications.Verify(n => n.AddAsync(It.IsAny<PushoverNotification>(), CancellationToken.None), Times.Once());
            _wishTable.VerifyBatch();
        }


        [Fact]
        public async Task ProviderOrchestrationAsync_Returns_Empty_List_With_No_Wishes()
        {
            var providerCtx = new SearchProviderContext
            {
                Provider = new Provider(),
                Wishes = Enumerable.Empty<Wish>()
            };
            var context = new Mock<DurableOrchestrationContextBase>(MockBehavior.Strict);
            context.Setup(c => c.GetInput<SearchProviderContext>()).Returns(providerCtx);

            var result = await _function.ProviderOrchestrationAsync(context.Object);

            context.Verify(c => c.GetInput<SearchProviderContext>(), Times.Once());
            context.Verify(c => c.CallActivityAsync<IEnumerable<WishResult>>("WishSearch", It.IsAny<object>()), Times.Never());
            Assert.Empty(result);
        }

        [Fact]
        public async Task ProviderOrchestrationAsync_Returns_Wish_Results()
        {
            var wishA = new Wish { Name = "wish", Query = "query" };
            var wishB = new Wish { Name = "wish two", Query = "w.2" };
            var providerCtx = new SearchProviderContext
            {
                Provider = new Provider(),
                Wishes = new[] { wishA, wishB }
            };
            var context = new Mock<DurableOrchestrationContextBase>(MockBehavior.Strict);
            context.Setup(c => c.GetInput<SearchProviderContext>()).Returns(providerCtx);
            context.SetupSequence(c => c.CallActivityAsync<IEnumerable<WishResult>>("WishSearch", It.IsAny<object>()))
                    .ReturnsAsync(new[]
                    {
                        new WishResult(),
                        new WishResult()
                    }).
                    ReturnsAsync(new[]
                    {
                        new WishResult()
                    });

            var result = await _function.ProviderOrchestrationAsync(context.Object);

            context.Verify(c => c.GetInput<SearchProviderContext>(), Times.Once());
            context.Verify(c => c.CallActivityAsync<IEnumerable<WishResult>>("WishSearch", It.IsAny<object>()), Times.Exactly(2));
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task PurgeSearchHistoryAsync_Purges_Batch_History()
        {
            var client = new Mock<DurableOrchestrationClientBase>();
            var timer = new TimerInfo(new DailySchedule(), new ScheduleStatus(), false);
            client.Setup(c => c.PurgeInstanceHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<OrchestrationStatus>>()))
                .ReturnsAsync(new PurgeHistoryResult(1));

            await _function.PurgeSearchHistoryAsync(timer, client.Object);

            client.Verify(c => c.PurgeInstanceHistoryAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<IEnumerable<OrchestrationStatus>>()), Times.Once());
        }
    }
}