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
    public class ProviderFunctions
    {
        [FunctionName("Add-Provider")]
        public async Task<IActionResult> AddProviderAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Post, Route = "provider")] HttpRequest req,
            [Table(Constants.ProviderTableName)] CloudTable table)
        {
            var (model, errors) = await req.GetRequestModel<ProviderViewModel, ProviderValidator>();
            if (model == null)
            {
                return errors.ToBadRequest();
            }

            var domainModel = model.ToDomainModel();
            var command = new AddProviderCommand(domainModel);

            await table.ExecuteAsync(command);

            var location = req.CreateLocation("/providers");
            return new CreatedResult(location, domainModel.ToViewModel());
        }

        [FunctionName("Get-Providers")]
        public async Task<IActionResult> GetProvidersAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Get, Route = "providers")] HttpRequest req,
            [Table(Constants.ProviderTableName)] CloudTable table)
        {
            var query = new GetProvidersQuery();

            var providers = await table.ExecuteAsync(query);

            return new OkObjectResult(providers.Select(p => p.ToViewModel()));
        }

        [FunctionName("Delete-Provider")]
        public async Task<IActionResult> DeleteProviderAsync(
            [HttpTrigger(AuthorizationLevel.Function, Constants.Delete, Route = "providers/{id}")] HttpRequest req,
            [Table(Constants.ProviderTableName)] CloudTable table,
            string id)
        {
            var command = new DeleteProviderCommand(id);

            await table.ExecuteAsync(command);

            return new NoContentResult();
        }
    }
}