using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Extensions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Azure.Validation;
using NzbWishlist.Core.Data;
using System.Linq;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Functions
{
    public class WishFunctions
    {
        [FunctionName("Add-Wish")]
        public async Task<IActionResult> AddWishAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Post, Route = "wish")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable table)
        {
            var (model, errors) = await req.GetRequestModel<WishViewModel, WishValidator>();
            if (model == null)
            {
                return errors.ToBadRequest();
            }

            var domainModel = model.ToDomainModel();
            var command = new AddWishCommand(domainModel);

            await command.ExecuteAsync(table);

            var location = req.CreateLocation("/wishes");
            return new CreatedResult(location, domainModel.ToViewModel());
        }

        [FunctionName("Get-Wishes")]
        public async Task<IActionResult> GetWishesAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Get, Route = "wishes")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable table)
        {
            var query = new GetWishesQuery();

            var wishes = await query.ExecuteAsync(table);

            return new OkObjectResult(wishes.Select(w => w.ToViewModel()));
        }

        [FunctionName("Get-WishResults")]
        public async Task<IActionResult> GetWishResultsAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Get, Route = "wishes/{id}/results")] HttpRequest req,
            string id,
            [Table(Constants.WishTableName)] CloudTable table)
        {
            var query = new GetWishResultsQuery(id);

            var results = await query.ExecuteAsync(table);

            return new OkObjectResult(results.Select(r => r.ToViewModel()));
        }

        [FunctionName("Delete-Wish")]
        public async Task<IActionResult> DeleteWishAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Delete, Route = "wishes/{id}")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable table,
            string id)
        {
            var command = new DeleteWishCommand(id);

            await command.ExecuteAsync(table);

            return new NoContentResult();
        }
    }
}