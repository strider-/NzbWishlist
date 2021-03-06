﻿using Microsoft.WindowsAzure.Storage.Table;
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
            var p = new Provider();
            _table.SetupOperation(p, TableOperationType.Insert);
            var cmd = new AddProviderCommand(p);

            await cmd.ExecuteAsync(_table.Object);

            _table.VerifyOperation(p, TableOperationType.Insert);
        }
    }
}
