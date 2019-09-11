using System.Threading.Tasks;

namespace NzbWishlist.Core
{
    public interface ICommandAsync<in TModel>
    {
        Task ExecuteAsync(TModel model);
    }
}
