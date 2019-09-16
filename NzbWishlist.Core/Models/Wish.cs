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

        public int DaysSinceLastSearch()
        {
            var days = (int)Math.Ceiling(DateTime.UtcNow.Subtract(LastSearchDate).TotalDays);
            return Math.Max(1, days);
        }

        public string Name { get; set; }

        public string Query { get; set; }

        public bool Active { get; set; }

        public DateTime LastSearchDate { get; set; }
    }
}