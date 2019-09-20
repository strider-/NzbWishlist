using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NzbWishlist.Core.Models
{
    public class CartEntry : TableEntity
    {
        public CartEntry()
        {
            PartitionKey = nameof(CartEntry);
            RowKey = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks,0:D20}";
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime PublishDate { get; set; }

        public string DetailsUrl { get; set; }
        
        public string GrabUrl { get; set; }

        public string NzbUrl { get; set; }

        public string Category { get; set; }
    }
}
