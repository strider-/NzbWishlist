using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Extensions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Azure.Validation;
using NzbWishlist.Core.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Functions
{
    public class WishFunctions
    {
        [FunctionName("Add-Wish")]
        public async Task<IActionResult> AddWishAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Post, Route = "wish")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable table,
            ILogger log)
        {
            try
            {
                var (model, errors) = await req.GetRequestModel<WishViewModel, WishValidator>();
                if (model == null)
                {
                    return errors.ToBadRequest();
                }

                var domainModel = model.ToDomainModel();
                var command = new AddWishCommand(domainModel);

                await table.ExecuteAsync(command);

                var location = req.CreateLocation("/wishes");
                return new CreatedResult(location, domainModel.ToViewModel());
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Add-Wish caused an exception.");
                return ex.ToUnprocessableResult();
            }
        }

        [FunctionName("Get-Wishes")]
        public async Task<IActionResult> GetWishesAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Get, Route = "wishes")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable table,
            ILogger log)
        {
            try
            {
                var query = new GetWishesQuery();

                var wishes = await table.ExecuteAsync(query);

                return new OkObjectResult(wishes.Select(w => w.ToViewModel()));
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Get-Wishes caused an exception");
                return ex.ToServerError();
            }
        }

        [FunctionName("Get-WishResults")]
        public async Task<IActionResult> GetWishResultsAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Get, Route = "wishes/{id}/results")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable table,
            ILogger log,
            string id)
        {
            try
            {
                var query = new GetWishResultsQuery(id);

                var results = await table.ExecuteAsync(query);

                return new OkObjectResult(results.Select(r => r.ToViewModel()));
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Get-WishResults caused an exception");
                return ex.ToServerError();
            }
        }

        [FunctionName("Delete-Wish")]
        public async Task<IActionResult> DeleteWishAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Delete, Route = "wishes/{id}")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable table,
            ILogger log,
            string id)
        {
            try
            {
                var command = new DeleteWishCommand(id);

                await table.ExecuteAsync(command);

                return new NoContentResult();
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Delete-Wish caused an exception");
                return ex.ToUnprocessableResult();
            }
        }

        [FunctionName("Toggle-Wish")]
        public async Task<IActionResult> ToggleWishAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Post, Route = "wishes/toggle")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable table,
            ILogger log)
        {
            try
            {
                var (model, errors) = await req.GetRequestModel<ToggleWishViewModel, ToggleWishValidator>();
                if (model == null)
                {
                    return errors.ToBadRequest();
                }

                var command = new ToggleWishCommand(model.WishId, model.Active.Value);

                await table.ExecuteAsync(command);

                return new NoContentResult();
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Toggle-Wish caused an exception");
                return ex.ToUnprocessableResult();
            }
        }
    }
}