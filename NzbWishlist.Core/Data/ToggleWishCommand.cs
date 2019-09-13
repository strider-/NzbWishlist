using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Data
{
    public class ToggleWishCommand : ICommandAsync<CloudTable>
    {
        private readonly string _wishId;
        private readonly bool _active;

        public ToggleWishCommand(string wishId, bool active)
        {
            _wishId = wishId;
            _active = active;
        }

        public async Task ExecuteAsync(CloudTable model)
        {
            var activeCol = nameof(Wish.Active);
            var retOp = TableOperation.Retrieve<DynamicTableEntity>(nameof(Wish), _wishId, new List<string>
            {
                activeCol
            });

            var result = await model.ExecuteAsync(retOp);
            if (result.Result == null)
            {
                throw new Exception($"Wish id {_wishId} does not exist.");
            }

            var entity = result.Result as DynamicTableEntity;
            entity.Properties[activeCol] = new EntityProperty(_active);

            var op = TableOperation.Merge(entity);

            await model.ExecuteAsync(op);
        }
    }
}