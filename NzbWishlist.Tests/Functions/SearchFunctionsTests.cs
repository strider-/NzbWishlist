using Moq;
using NzbWishlist.Azure.Functions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Core.Services;
using NzbWishlist.Tests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Functions
{
    [Trait(nameof(Functions), nameof(SearchFunctions))]
    public class SearchFunctionsTests
    {
        private readonly SearchFunctions _function;
        private readonly Mock<INewznabClient> _client = new Mock<INewznabClient>(MockBehavior.Strict);
        private readonly MockLogger _log = new MockLogger();

        public SearchFunctionsTests()
        {
            _function = new SearchFunctions(_client.Object);
        }

        [Fact]
        public async Task WishSearchAsync_Invokes_The_NewznabClient()
        {
            var ctx = new SearchWishContext
            {
                Provider = new Provider(),
                Wish = new Wish()
            };
            _client.Setup(c => c.SearchAsync(ctx.Provider, ctx.Wish))
                   .ReturnsAsync(Enumerable.Empty<WishResult>());

            await _function.WishSearchAsync(ctx, _log.Object);

            _client.Verify(c => c.SearchAsync(ctx.Provider, ctx.Wish), Times.Once());
        }

        [Fact]
        public async Task WishSearchAsync_Associates_Wish_Results_To_Their_Wish()
        {
            var ctx = new SearchWishContext
            {
                Provider = new Provider(),
                Wish = new Wish { Name = "A Wish" }
            };
            _client.Setup(c => c.SearchAsync(ctx.Provider, ctx.Wish))
                   .ReturnsAsync(new[] { new WishResult { } });

            var results = await _function.WishSearchAsync(ctx, _log.Object);

            _client.Verify(c => c.SearchAsync(ctx.Provider, ctx.Wish), Times.Once());
            Assert.All(results, r =>
            {
                Assert.Equal("A Wish", r.WishName);
                Assert.NotNull(r.RowKey);
            });
        }

        [Fact]
        public async Task WishSearchAsync_Logs_Exceptions()
        {
            var ctx = new SearchWishContext
            {
                Provider = new Provider(),
                Wish = new Wish()
            };
            _client.Setup(c => c.SearchAsync(ctx.Provider, ctx.Wish)).ThrowsAsync(new Exception("uh oh!"));

            var results = await _function.WishSearchAsync(ctx, _log.Object);

            _log.VerifyLoggedException("NewznabClient caused an exception");
            Assert.Empty(results);
        }
    }
}