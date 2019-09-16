using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Functions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Functions
{
    [Trait(nameof(Functions), nameof(WishFunctions))]
    public class WishFunctionsTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();
        private readonly WishFunctions _function = new WishFunctions();
        private readonly MockLogger _log = new MockLogger();


        [Fact]
        public async Task AddWishAsync_Returns_BadRequest_When_An_Invalid_Model()
        {
            var req = TestHelper.CreateHttpRequest(new { });

            var resp = await _function.AddWishAsync(req, _table.Object, _log.Object);

            Assert.IsType<BadRequestObjectResult>(resp);
        }

        [Fact]
        public async Task AddWishAsync_Returns_Unprocessable_When_An_Exception_Is_Thrown()
        {
            var req = TestHelper.CreateHttpRequest(new
            {
                name = "new wish",
                query = "new.wish"
            });
            _table.SetupOperationToThrow();

            var resp = await _function.AddWishAsync(req, _table.Object, _log.Object);

            _log.VerifyLoggedException("Add-Wish caused an exception");
            Assert.IsType<UnprocessableEntityObjectResult>(resp);
        }

        [Fact]
        public async Task AddWishAsync_Returns_Created_With_The_New_Wish()
        {
            var reqModel = new WishViewModel
            {
                Name = "a thing",
                Query = "a.thing",
                Active = false
            };
            var req = TestHelper.CreateHttpRequest(reqModel);
            _table.SetupOperation(TableOperationType.Insert, () => new Wish
            {
                Name = reqModel.Name,
                Query = reqModel.Query,
                Active = reqModel.Active.Value
            });

            var resp = await _function.AddWishAsync(req, _table.Object, _log.Object);

            _table.VerifyOperation(TableOperationType.Insert);
            var cr = Assert.IsType<CreatedResult>(resp);
            var model = Assert.IsType<WishViewModel>(cr.Value);
            Assert.NotNull(model.Id);
            Assert.Equal(reqModel.Name, model.Name);
            Assert.Equal(reqModel.Query, model.Query);
            Assert.Equal(reqModel.Active, model.Active);
        }

        [Fact]
        public async Task GetWishesAsync_Returns_ServerError_When_An_Exception_Is_Thrown()
        {
            var req = TestHelper.CreateHttpRequest();
            _table.SetupOperationToThrow();

            var resp = await _function.GetWishesAsync(req, _table.Object, _log.Object);

            _log.VerifyLoggedException("Get-Wishes caused an exception");
            var objResult = Assert.IsType<ObjectResult>(resp);
            Assert.Equal(500, objResult.StatusCode);
        }

        [Fact]
        public async Task GetWishesAsync_Returns_All_Available_Wishes()
        {
            var req = TestHelper.CreateHttpRequest();
            _table.SetupSegmentedQuery(new[]
            {
                new Wish { Name = "Test", Query = "test", Active = true },
                new Wish { Name = "Test 2", Query = "test.2", Active = false },
            }.ToList());

            var resp = await _function.GetWishesAsync(req, _table.Object, _log.Object);

            _table.VerifySegmentedQuery<Wish>();
            var okResult = Assert.IsType<OkObjectResult>(resp);
            var providers = Assert.IsAssignableFrom<IEnumerable<WishViewModel>>(okResult.Value);
            Assert.Collection(providers,
                p => Assert.Equal("Test", p.Name),
                p => Assert.Equal("Test 2", p.Name));
        }

        [Fact]
        public async Task DeleteWishAsync_Returns_Unprocessable_When_An_Exception_Is_Thrown()
        {
            var req = TestHelper.CreateHttpRequest();
            _table.SetupSegmentedQueryToThrow();

            var resp = await _function.DeleteWishAsync(req, _table.Object, _log.Object, "123");

            _log.VerifyLoggedException("Delete-Wish caused an exception");
            Assert.IsType<UnprocessableEntityObjectResult>(resp);
        }

        [Fact]
        public async Task DeleteWishAsync_Returns_NoContent_When_Successful()
        {
            var req = TestHelper.CreateHttpRequest();
            _table.SetupSegmentedQuery(Enumerable.Empty<DynamicTableEntity>());
            _table.SetupOperation<Wish>(TableOperationType.Delete);

            var resp = await _function.DeleteWishAsync(req, _table.Object, _log.Object, "123");

            Assert.IsType<NoContentResult>(resp);
            _table.VerifyOperation(TableOperationType.Delete);
        }

        [Fact]
        public async Task ToggleWishAsync_Returns_BadRequest_With_An_Invalid_Model()
        {
            var req = TestHelper.CreateHttpRequest(new { });

            var resp = await _function.ToggleWishAsync(req, _table.Object, _log.Object);

            Assert.IsType<BadRequestObjectResult>(resp);
        }

        [Fact]
        public async Task ToggleWishAsync_Returns_Unprocessable_When_An_Exception_Is_Thrown()
        {
            var id = "123";
            var req = TestHelper.CreateHttpRequest(new
            {
                wishId = id,
                active = false
            });
            _table.SetupOperation(TableOperationType.Retrieve, () => new DynamicTableEntity
            {
                Properties = new Dictionary<string, EntityProperty>() {
                    {"Active", new EntityProperty(true) }
                },
                RowKey = id,
                ETag = "*"
            });
            _table.SetupOperationToThrow(TableOperationType.Merge);

            var resp = await _function.ToggleWishAsync(req, _table.Object, _log.Object);

            _log.VerifyLoggedException("Toggle-Wish caused an exception");
            _table.VerifyOperation(TableOperationType.Retrieve);
            Assert.IsType<UnprocessableEntityObjectResult>(resp);
        }

        [Fact]
        public async Task ToggleWishAsync_Returns_NoContent_When_Successful()
        {
            var id = "123";
            var req = TestHelper.CreateHttpRequest(new
            {
                wishId = id,
                active = false
            });
            _table.SetupOperation(TableOperationType.Retrieve, () => new DynamicTableEntity
            {
                Properties = new Dictionary<string, EntityProperty>() {
                    {"Active", new EntityProperty(true) }
                },
                RowKey = id,
                ETag = "*"
            });
            _table.SetupOperation<DynamicTableEntity>(TableOperationType.Merge);

            var resp = await _function.ToggleWishAsync(req, _table.Object, _log.Object);

            _table.VerifyOperation(TableOperationType.Retrieve);
            _table.VerifyOperation(TableOperationType.Merge);
            Assert.IsType<NoContentResult>(resp);
        }
    }
}
