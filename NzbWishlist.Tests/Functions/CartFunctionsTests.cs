using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using NzbWishlist.Azure.Functions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Core.Services;
using NzbWishlist.Tests.Fixtures;
using System;
using System.IO;
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
        private readonly MockLogger _log = new MockLogger();
        private readonly Mock<INewznabClient> _client = new Mock<INewznabClient>(MockBehavior.Strict);
        private readonly CartFunctions _function;

        public CartFunctionsTests()
        {
            _function = new CartFunctions(_client.Object);
        }

        [Fact]
        public async Task RssAsync_Returns_An_Rss_2_Point_0_Feed_And_Passes_The_QueryString_Through_To_Links()
        {
            var req = TestHelper.CreateHttpRequest("https://nzb.mtighe.dev/api/cart/rss?del=1");
            _cartTable.SetupSegmentedQuery(new[] {
                new CartEntry { Category = "Cat", Description = "", DetailsUrl = "https://no.where/details/cat", GrabUrl = "https://no.where/nzb/cat", Title = "item 1" },
                new CartEntry { Category = "Dog", Description = "", DetailsUrl = "https://no.where/details/dog", GrabUrl = "https://no.where/nzb/dog", Title = "item 2" },
            });

            var result = await _function.RssAsync(req, _cartTable.Object);

            var cr = Assert.IsType<ContentResult>(result);
            var doc = XDocument.Parse(cr.Content);
            var items = doc.Root.Element("channel").Elements("item");
            Assert.Equal(200, cr.StatusCode);
            Assert.Equal("text/xml", cr.ContentType);
            Assert.Equal("rss", doc.Root.Name.LocalName);
            Assert.Equal("2.0", doc.Root.Attribute("version").Value);
            Assert.Equal(2, items.Count());
            Assert.All(items, x =>
            {
                Assert.EndsWith("?del=1", x.Element("link").Value);
                Assert.EndsWith("?del=1", x.Element("enclosure").Attribute("url").Value);
            });
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

        [Fact]
        public async Task GrabNzbFromCartAsync_Returns_Not_Found_With_Invalid_Entry()
        {
            var req = TestHelper.CreateHttpRequest("https://nzb.mtighe.dev/api/cart/nzb/123");
            _cartTable.SetupOperationToFail(TableOperationType.Retrieve);

            var result = await _function.GrabNzbFromCartAsync(req, _cartTable.Object, _log.Object, "123");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GrabNzbFromCartAsync_Returns_Not_Found_When_The_Nzb_Could_Not_Be_Grabbed()
        {
            var req = TestHelper.CreateHttpRequest("https://nzb.mtighe.dev/api/cart/nzb/123");
            var entry = new CartEntry();
            _cartTable.SetupOperation(TableOperationType.Retrieve, () => entry);
            _client.Setup(c => c.GetNzbStreamAsync(entry)).ReturnsAsync((Stream)null);

            var result = await _function.GrabNzbFromCartAsync(req, _cartTable.Object, _log.Object, "123");

            _cartTable.VerifyOperation(TableOperationType.Retrieve);
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GrabNzbFromCartAsync_Returns_An_Nzb_File_Stream_Result()
        {
            var req = TestHelper.CreateHttpRequest("https://nzb.mtighe.dev/api/cart/nzb/123");
            var entry = new CartEntry();
            _cartTable.SetupOperation(TableOperationType.Retrieve, () => entry);
            _client.Setup(c => c.GetNzbStreamAsync(entry)).ReturnsAsync(new MemoryStream());

            var result = await _function.GrabNzbFromCartAsync(req, _cartTable.Object, _log.Object, "123");

            _cartTable.VerifyOperation(TableOperationType.Retrieve);
            var fileResult = Assert.IsType<FileStreamResult>(result);
            Assert.Equal("application/x+nzb", fileResult.ContentType);
        }

        [Fact]
        public async Task GrabNzbFromCartAsync_Respects_The_QueryString()
        {
            var req = TestHelper.CreateHttpRequest("https://nzb.mtighe.dev/api/cart/nzb/123?del=1");
            var entry = new CartEntry { ETag = "*" };
            _cartTable.SetupOperation(TableOperationType.Retrieve, () => entry);
            _cartTable.SetupOperation(entry, TableOperationType.Delete);
            _client.Setup(c => c.GetNzbStreamAsync(entry)).ReturnsAsync(new MemoryStream());

            var result = await _function.GrabNzbFromCartAsync(req, _cartTable.Object, _log.Object, "123");

            _cartTable.VerifyOperation(TableOperationType.Retrieve);
            _cartTable.VerifyOperation(TableOperationType.Delete);
        }

        [Fact]
        public async Task GrabNzbFromCartAsync_Returns_ServerError_When_An_Exception_Is_Thrown()
        {
            var req = TestHelper.CreateHttpRequest("https://nzb.mtighe.dev/api/cart/nzb/123");
            _cartTable.SetupOperationToThrow();

            var result = await _function.GrabNzbFromCartAsync(req, _cartTable.Object, _log.Object, "123");

            _log.VerifyLoggedException("Cart-Grab caused an exception");
            var objResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objResult.StatusCode);
        }
    }
}
