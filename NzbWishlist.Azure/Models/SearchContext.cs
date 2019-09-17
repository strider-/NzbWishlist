using NzbWishlist.Core.Models;
using System.Collections.Generic;

namespace NzbWishlist.Azure.Models
{
    internal class SearchContext
    {
        public IEnumerable<Provider> Providers { get; set; }

        public IEnumerable<Wish> Wishes { get; set; }
    }
}
