using NzbWishlist.Core.Models;

namespace NzbWishlist.Core.Data
{
    public class AddWishCommand : AddEntityCommand<Wish>
    {
        public AddWishCommand(Wish wish) : base(wish) { }
    }
}
