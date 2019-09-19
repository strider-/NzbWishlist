using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Functions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace NzbWishlist.Tests.Functions
{
    [Trait(nameof(Functions), nameof(CartFunctions))]
    public class CartFunctionsTests
    {
        private readonly MockCloudTable _cartTable = new MockCloudTable();
        private readonly CartFunctions _function = new CartFunctions();
        private readonly MockLogger _log = new MockLogger();

        [Fact]
        public async Task RssAsync_Returns_An_Rss_2_Point_0_Feed()
        {
            var req = TestHelper.CreateHttpRequest("https://nzb.mtighe.dev/api/cart/rss");
            _cartTable.SetupSegmentedQuery(new[] {
                new CartEntry { Category = "Cat", Description = "", DetailsUrl = "https://no.where/details/cat", GrabUrl = "https://no.where/nzb/cat", Title = "item 1" },
                new CartEntry { Category = "Dog", Description = "", DetailsUrl = "https://no.where/details/dog", GrabUrl = "https://no.where/nzb/dog", Title = "item 2" },
            });

            var result = await _function.RssAsync(req, _cartTable.Object);

            var cr = Assert.IsType<ContentResult>(result);
            var doc = XDocument.Parse(cr.Content);
            Assert.Equal(200, cr.StatusCode);
            Assert.Equal("text/xml", cr.ContentType);
            Assert.Equal("rss", doc.Root.Name.LocalName);
            Assert.Equal("2.0", doc.Root.Attribute("version").Value);
            Assert.Equal(2, doc.Root.Element("channel").Elements("item").Count());
        }

        [Fact]
        public async Task AddToCartAsync_Returns_Unprocessable_When_Exceptions_Are_Thrown()
        {
            var req = TestHelper.CreateHttpRequest("https://nzb.mtighe.dev/api/cart/add/123");
            var wishTable = new MockCloudTable();
            wishTable.SetupOperationToThrow();

            var result = await _function.AddToCartAsync(req, wishTable.Object, _cartTable.Object, _log.Object, "123");

            _log.VerifyLoggedException("Cart-Add caused an exception");
            Assert.IsType<UnprocessableEntityObjectResult>(result);
        }

        [Fact]
        public async Task AddToCartAsync_Returns_CreatedResult()
        {
            var id = "123";
            var req = TestHelper.CreateHttpRequest($"https://nzb.mtighe.dev/api/cart/add/{id}");
            var wishTable = new MockCloudTable();
            var wishResult = new WishResult
            {
                Category = "Cat",
                DetailsUrl = "https://no.where/details/123",
                Title = "wish result",
                NzbUrl = "https://no.where/nzb/123",
                PubDate = DateTime.UtcNow.AddDays(-10)
            };
            wishTable.SetupOperation(TableOperationType.Retrieve, () => wishResult);
            _cartTable.SetupOperation<CartEntry>(TableOperationType.Insert);

            var result = await _function.AddToCartAsync(req, wishTable.Object, _cartTable.Object, _log.Object, id);

            wishTable.VerifyOperation(TableOperationType.Retrieve);
            _cartTable.VerifyOperation(TableOperationType.Insert);
            var created = Assert.IsType<CreatedResult>(result);
            Assert.Equal("https://nzb.mtighe.dev:443/api/cart/rss", created.Location);
            var vm = Assert.IsType<CartEntryViewModel>(created.Value);
            Assert.Equal(wishResult.Category, vm.Category);
            Assert.Equal(wishResult.DetailsUrl, vm.DetailsUrl);
            Assert.Equal(wishResult.Title, vm.Title);
            Assert.Equal($"https://nzb.mtighe.dev:443/api/cart/nzb/{vm.Id}", vm.GrabUrl);
            Assert.Equal(wishResult.PubDate, vm.PublishDate);
            Assert.Equal("", vm.Description);
        }
    }
}
