using NzbWishlist.Core.Models;

namespace NzbWishlist.Core.Data
{
    public class AddProviderCommand : AddEntityCommand<Provider>
    {
        public AddProviderCommand(Provider provider) : base(provider) { }
    }
}
