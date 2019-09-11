using NzbWishlist.Core.Models;

namespace NzbWishlist.Core.Data
{
    public class GetProvidersQuery : GetEntitiesQuery<Provider>
    {
        public GetProvidersQuery() : base(nameof(Provider)) { }
    }
}
