using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Extensions;
using NzbWishlist.Core.Data;
using System;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Functions
{
    public class CartFunctions
    {
        [FunctionName("Cart-RSS")]
        public IActionResult Rss(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Get, Route = "cart/rss")] HttpRequest req,
            [Table(Constants.CartTableName)] CloudTable table)
        {
            throw new NotImplementedException();
        }

        [FunctionName("Cart-Add")]
        public async Task<IActionResult> AddToCartAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Post, Route = "cart/add/{wishResultId}")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable wishTable,
            [Table(Constants.CartTableName)] CloudTable cartTable,
            ILogger log,
            string wishResultId)
        {
            try
            {
                var wishResult = await wishTable.ExecuteAsync(new GetWishResultQuery(wishResultId));

                var entry = wishResult.ToCartEntry(id => req.CreateLocation($"/cart/nzb/{id}", includeQuery: true));

                await cartTable.ExecuteAsync(new AddToCartCommand(entry));

                return new CreatedResult(req.CreateLocation("/cart/rss"), entry.ToViewModel());
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Cart-Add caused an exception.");
                return ex.ToUnprocessableResult();
            }
        }

        [FunctionName("Cart-Grab")]
        public async Task<IActionResult> GrabNzbFromCartAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Get, Route = "cart/nzb/{cartId}")] HttpRequest req,
            [Table(Constants.CartTableName)] CloudTable table,
            string cartId)
        {
            throw new NotImplementedException();
        }
    }
}