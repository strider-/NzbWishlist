﻿using Moq;
using NzbWishlist.Azure.Functions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Core.Services;
using System.Linq;
using Xunit;

namespace NzbWishlist.Tests.Functions
{
    [Trait(nameof(Functions), nameof(SearchFunctions))]
    public class SearchFunctionsTests
    {
        private readonly SearchFunctions _function;
        private readonly Mock<INewznabClient> _client = new Mock<INewznabClient>(MockBehavior.Strict);

        public SearchFunctionsTests()
        {
            _function = new SearchFunctions(_client.Object);
        }

        [Fact]
        public async void WishSearchAsync_Invokes_The_NewznabClient()
        {
            var ctx = new SearchWishContext
            {
                Provider = new Provider(),
                Wish = new Wish()
            };
            _client.Setup(c => c.SearchAsync(ctx.Provider, ctx.Wish))
                   .ReturnsAsync(Enumerable.Empty<WishResult>());

            await _function.WishSearchAsync(ctx);

            _client.Verify(c => c.SearchAsync(ctx.Provider, ctx.Wish), Times.Once());
        }

        [Fact]
        public async void WishSearchAsync_Associates_Wish_Results_To_Their_Wish()
        {
            var ctx = new SearchWishContext
            {
                Provider = new Provider(),
                Wish = new Wish { Name = "A Wish" }
            };
            _client.Setup(c => c.SearchAsync(ctx.Provider, ctx.Wish))
                   .ReturnsAsync(new[] { new WishResult { } });

            var results = await _function.WishSearchAsync(ctx);

            _client.Verify(c => c.SearchAsync(ctx.Provider, ctx.Wish), Times.Once());
            Assert.All(results, r =>
            {
                Assert.Equal("A Wish", r.WishName);
                Assert.NotNull(r.RowKey);
            });
        }
    }
}