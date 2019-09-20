using Microsoft.Extensions.Primitives;
using NzbWishlist.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Services
{
    public interface INewznabClient
    {
        Task<IEnumerable<WishResult>> SearchAsync(Provider provider, Wish wish);

        Task<(Stream, IEnumerable<KeyValuePair<string, StringValues>>)> GetNzbStreamAsync(CartEntry entry);
    }
}
