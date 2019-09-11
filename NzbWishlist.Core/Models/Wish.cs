using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NzbWishlist.Core.Models
{
    public class Wish : TableEntity
    {
        public Wish()
        {
            PartitionKey = nameof(Wish);
            RowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks,0:D20}";
        }

        public string Name { get; set; }

        public string Query { get; set; }

        public bool Active { get; set; }
    }
}
