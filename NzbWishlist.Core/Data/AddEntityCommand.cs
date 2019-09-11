using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public abstract class AddEntityCommand<T> : ICommandAsync<CloudTable> where T : ITableEntity
    {
        private T _entity;

        public AddEntityCommand(T entity) => _entity = entity;

        public virtual async Task ExecuteAsync(CloudTable model)
        {
            var op = TableOperation.Insert(_entity);

            var result = await model.ExecuteAsync(op);

            _entity = (T)result.Result;
        }
    }
}
