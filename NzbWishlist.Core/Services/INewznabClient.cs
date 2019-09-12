using NzbWishlist.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Services
{
    public interface INewznabClient
    {
        Task<IEnumerable<WishResult>> SearchAsync(Provider provider, Wish wish, int maxAgeInDays = 0);
    }
}
