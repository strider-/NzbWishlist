using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace NzbWishlist.Core.Models
{
    public
    class WishResult : TableEntity
    {
        public WishResult()
        {
            PartitionKey = nameof(WishResult);
            Id = $"{DateTime.MaxValue.Ticks - DateTime.UtcNow.Ticks,0:D20}";
        }

        public void BelongsTo(Wish wish)
        {
            RowKey = $"{wish.RowKey}_{Id}";
        }

        public string Id { get; set; }

        public string Title { get; set; }

        public long Size { get; set; }

        public DateTime PubDate { get; set; }

        public string DetailsUrl { get; set; }

        public string PreviewUrl { get; set; }

        public string NzbUrl { get; set; }
    }
}
