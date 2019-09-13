using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Extensions
{
    public static class CloudTableExtensions
    {
        public static async Task<TResult> ExecuteAsync<TResult>(this CloudTable table, IQueryAsync<CloudTable, TResult> query)
        {
            return await query.ExecuteAsync(table);
        }

        public static async Task ExecuteAsync(this CloudTable table, ICommandAsync<CloudTable> command)
        {
            await command.ExecuteAsync(table);
        }
    }
}
