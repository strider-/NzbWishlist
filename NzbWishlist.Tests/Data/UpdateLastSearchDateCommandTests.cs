using NzbWishlist.Core.Data;
using NzbWishlist.Core.Models;
using NzbWishlist.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NzbWishlist.Tests.Data
{
    [Trait(nameof(Data), nameof(UpdateLastSearchDateCommand))]
    public class UpdateLastSearchDateCommandTests
    {
        private readonly MockCloudTable _table = new MockCloudTable();

        [Fact]
        public async Task ExecuteAsync_Updates_The_Last_Search_Date_For_Given_Wishes()
        {
            var wishes = new List<Wish>
            {
                new Wish { LastSearchDate = DateTime.MinValue },
                new Wish { LastSearchDate = DateTime.MinValue }
            };
            _table.SetupBatch();

            var cmd = new UpdateLastSearchDateCommand(wishes);
            await cmd.ExecuteAsync(_table.Object);

            _table.VerifyBatch();
            Assert.All(wishes, w => Assert.NotEqual(w.LastSearchDate, DateTime.MinValue));
        }
    }
}