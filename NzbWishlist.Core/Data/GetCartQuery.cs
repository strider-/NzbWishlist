using NzbWishlist.Core.Models;

namespace NzbWishlist.Core.Data
{
    public class GetCartQuery : GetEntitiesQuery<CartEntry>
    {
        public GetCartQuery() : base(nameof(CartEntry)) { }
    }
}