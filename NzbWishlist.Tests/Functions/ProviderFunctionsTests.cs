using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using NzbWishlist.Azure.Functions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Azure.Services;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Functions
{
    [Trait(nameof(Functions), nameof(ProviderFunctions))]
    public class ProviderFunctionsTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();
        private readonly ProviderFunctions _function;
        private readonly Mock<IAuthService> _authService = new Mock<IAuthService>(MockBehavior.Strict);
        private readonly MockLogger _log = new MockLogger();

        public ProviderFunctionsTests()
        {
            _function = new ProviderFunctions(_authService.Object);
        }

        [Fact]
        public async Task AddProviderAsync_Requires_Authentication()
        {
            var req = TestHelper.CreateHttpRequest(new { });
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(false);

            var resp = await _function.AddProviderAsync(req, _table.Object, _log.Object);

            Assert.IsType<UnauthorizedResult>(resp);
        }

        [Fact]
        public async Task AddProviderAsync_Returns_BadRequest_With_An_Invalid_Model()
        {
            var req = TestHelper.CreateHttpRequest(new { });
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(true);

            var resp = await _function.AddProviderAsync(req, _table.Object, _log.Object);

            Assert.IsType<BadRequestObjectResult>(resp);
        }

        [Fact]
        public async Task AddProviderAsync_Returns_Unprocessable_When_Exceptions_Are_Thrown()
        {
            var req = TestHelper.CreateHttpRequest(new
            {
                name = "Provider",
                apiKey = "abc-123",
                apiUrl = "https://no.where"
            });
            _table.SetupOperationToThrow();
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(true);

            var resp = await _function.AddProviderAsync(req, _table.Object, _log.Object);

            _log.VerifyLoggedException("Add-Provider caused an exception");
            Assert.IsType<UnprocessableEntityObjectResult>(resp);
        }

        [Fact]
        public async Task AddProviderAsync_Returns_A_Created_Result_With_The_New_Provider()
        {
            var reqModel = new ProviderViewModel
            {
                Name = "Provider",
                ApiKey = "abc-123",
                ApiUrl = "https://no.where",
                ImageDomain = "images.no.where"
            };
            var req = TestHelper.CreateHttpRequest(reqModel);
            _table.SetupOperation(TableOperationType.Insert, () => new Provider
            {
                Name = reqModel.Name,
                ApiKey = reqModel.ApiKey,
                ApiUrl = reqModel.ApiUrl,
                ImageDomain = reqModel.ImageDomain
            });
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(true);

            var resp = await _function.AddProviderAsync(req, _table.Object, _log.Object);

            _table.VerifyOperation(TableOperationType.Insert);
            var cr = Assert.IsType<CreatedResult>(resp);
            var model = Assert.IsType<ProviderViewModel>(cr.Value);
            Assert.NotNull(model.Id);
            Assert.Equal(reqModel.Name, model.Name);
            Assert.Equal(reqModel.ApiKey, model.ApiKey);
            Assert.Equal(reqModel.ApiUrl, model.ApiUrl);
            Assert.Equal(reqModel.ImageDomain, model.ImageDomain);
        }

        [Fact]
        public async Task GetProvidersAsync_Requires_Authentication()
        {
            var req = TestHelper.CreateHttpRequest();
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(false);

            var resp = await _function.GetProvidersAsync(req, _table.Object, _log.Object);

            Assert.IsType<UnauthorizedResult>(resp);
        }

        [Fact]
        public async Task GetProvidersAsync_Returns_ServerError_When_An_Exception_Is_Thrown()
        {
            var req = TestHelper.CreateHttpRequest();
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(true);
            _table.SetupOperationToThrow();

            var resp = await _function.GetProvidersAsync(req, _table.Object, _log.Object);

            _log.VerifyLoggedException("Get-Providers caused an exception");
            var objResult = Assert.IsType<ObjectResult>(resp);
            Assert.Equal(500, objResult.StatusCode);
        }

        [Fact]
        public async Task GetProvidersAsync_Returns_Available_Providers()
        {
            var req = TestHelper.CreateHttpRequest();
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(true);
            _table.SetupSegmentedQuery(new[]
            {
                new Provider { Name = "Test", ApiKey = "key", ApiUrl = "url" },
                new Provider { Name = "Test 2", ApiKey = "key", ApiUrl = "url" },
            }.ToList());

            var resp = await _function.GetProvidersAsync(req, _table.Object, _log.Object);

            _table.VerifySegmentedQuery<Provider>();
            var okResult = Assert.IsType<OkObjectResult>(resp);
            var providers = Assert.IsAssignableFrom<IEnumerable<ProviderViewModel>>(okResult.Value);
            Assert.Collection(providers,
                p => Assert.Equal("Test", p.Name),
                p => Assert.Equal("Test 2", p.Name));
        }

        [Fact]
        public async Task DeleteProviderAsync_Requires_Authentication()
        {
            var req = TestHelper.CreateHttpRequest();
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(false);

            var resp = await _function.DeleteProviderAsync(req, _table.Object, _log.Object, "123");

            Assert.IsType<UnauthorizedResult>(resp);
        }

        [Fact]
        public async Task DeleteProviderAsync_Returns_Unprocessable_When_An_Exception_Is_Thrown()
        {
            var req = TestHelper.CreateHttpRequest();
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(true);
            _table.SetupOperationToThrow();

            var resp = await _function.DeleteProviderAsync(req, _table.Object, _log.Object, "123");

            _log.VerifyLoggedException("Delete-Provider caused an exception");
            Assert.IsType<UnprocessableEntityObjectResult>(resp);
        }

        [Fact]
        public async Task DeleteProviderAsync_Returns_NoContent_When_Successful()
        {
            var req = TestHelper.CreateHttpRequest();
            _authService.Setup(s => s.IsAuthenticated(req)).ReturnsAsync(true);
            _table.SetupOperation<Provider>(TableOperationType.Delete);

            var resp = await _function.DeleteProviderAsync(req, _table.Object, _log.Object, "123");

            _table.VerifyOperation(TableOperationType.Delete);
            Assert.IsType<NoContentResult>(resp);
        }
    }
}