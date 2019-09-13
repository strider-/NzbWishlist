using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(AddProviderCommand))]
    public class AddProviderCommandTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Persists_A_New_Provider()
        {
            var p = new Provider { ApiKey = "k", ApiUrl = "u", ImageDomain = "x" };
            var cmd = new AddProviderCommand(p);
            _table.Setup(t => t.ExecuteAsync(It.IsAny<TableOperation>())).ReturnsAsync(new TableResult
            {
                Etag = "new!",
                HttpStatusCode = 200,
                Result = p
            });

            await cmd.ExecuteAsync(_table.Object);

            _table.Verify(t => t.ExecuteAsync(It.IsAny<TableOperation>()), Times.Once());
        }
    }
}
