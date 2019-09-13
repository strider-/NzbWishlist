using Microsoft.WindowsAzure.Storage.Table;
using Moq;
using System;

namespace NzbWishlist.Tests.Fixtures
{
    public class MockCloudTable : Mock<CloudTable>
    {
        public MockCloudTable() : base(MockBehavior.Strict, new[] { new Uri("https://no.where/devstoreaccount1/") })
        {

        }
    }
}
