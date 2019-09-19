using NzbWishlist.Core.Models;

namespace NzbWishlist.Core.Data
{
    public class AddToCartCommand : AddEntityCommand<CartEntry>
    {
        public AddToCartCommand(CartEntry entry) : base(entry) { }
    }
}
