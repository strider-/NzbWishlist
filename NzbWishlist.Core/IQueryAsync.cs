using System.Threading.Tasks;

namespace NzbWishlist.Core
{
    public interface IQueryAsync<in TModel, TResult>
    {
        Task<TResult> ExecuteAsync(TModel model);
    }
}