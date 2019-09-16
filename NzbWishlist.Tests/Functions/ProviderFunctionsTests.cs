using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using NzbWishlist.Azure.Functions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Functions
{
    [Trait(nameof(Functions), nameof(ProviderFunctions))]
    public class ProviderFunctionsTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();
        private readonly ProviderFunctions _function = new ProviderFunctions();
        private readonly ILogger _log = Mock.Of<ILogger>();

        [Fact]
        public async Task AddProviderAsync_Returns_BadRequest_With_An_Invalid_Model()
        {
            var req = TestHelper.CreateHttpRequest(new { });

            var resp = await _function.AddProviderAsync(req, _table.Object, _log);

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
            _table.Setup(t => t.ExecuteAsync(It.IsAny<TableOperation>())).ThrowsAsync(new Exception("uh oh"));

            var resp = await _function.AddProviderAsync(req, _table.Object, _log);

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

            var resp = await _function.AddProviderAsync(req, _table.Object, _log);

            _table.VerifyOperation(TableOperationType.Insert);
            var cr = Assert.IsType<CreatedResult>(resp);
            var model = Assert.IsType<ProviderViewModel>(cr.Value);
            Assert.NotNull(model.Id);
            Assert.Equal(reqModel.Name, model.Name);
            Assert.Equal(reqModel.ApiKey, model.ApiKey);
            Assert.Equal(reqModel.ApiUrl, model.ApiUrl);
            Assert.Equal(reqModel.ImageDomain, model.ImageDomain);
        }
    }
}