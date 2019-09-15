using NzbWishlist.Core.Models;

namespace NzbWishlist.Azure.Models
{
    public class SearchWishContext
    {
        public Provider Provider { get; set; }

        public Wish Wish { get; set; }
    }
}
