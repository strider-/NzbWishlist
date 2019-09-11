using NzbWishlist.Core.Models;

namespace NzbWishlist.Core.Data
{
    public class GetWishesQuery : GetEntitiesQuery<Wish>
    {
        public GetWishesQuery() : base(nameof(Wish)) { }
    }
}
