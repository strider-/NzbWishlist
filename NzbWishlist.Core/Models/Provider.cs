using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NzbWishlist.Core.Models
{
    public class Provider : TableEntity
    {
        public Provider()
        {
            PartitionKey = nameof(Provider);
            RowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks,0:D20}";
        }

        public string ApiUrl { get; set; }

        public string ApiKey { get; set; }

        public string ImageDomain { get; set; }
    }
}
