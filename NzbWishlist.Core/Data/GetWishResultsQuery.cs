using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;

namespace NzbWishlist.Core.Data
{
    public class GetWishResultsQuery : GetEntitiesQuery<WishResult>
    {
        public GetWishResultsQuery(string wishRowKey) : base(wishRowKey) { }

        protected override TableQuery<WishResult> Query()
        {
            return new TableQuery<WishResult>()
                .Where(new RowKeyStartsWithFilter(nameof(WishResult), PartitionKeyFilter).ToString());
        }
    }
}
