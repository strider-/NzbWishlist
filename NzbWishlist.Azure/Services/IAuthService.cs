using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Services
{
    public interface IAuthService
    {
        Task<bool> IsAuthenticated(HttpRequest request);
    }
}
