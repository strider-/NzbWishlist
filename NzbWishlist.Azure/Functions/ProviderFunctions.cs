﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Extensions;
using NzbWishlist.Azure.Models;
using NzbWishlist.Azure.Services;
using NzbWishlist.Azure.Validation;
using NzbWishlist.Core.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NzbWishlist.Azure.Functions
{
    public class ProviderFunctions
    {
        private readonly IAuthService _authService;

        public ProviderFunctions(IAuthService authService) => _authService = authService;

        [FunctionName("Add-Provider")]
        public async Task<IActionResult> AddProviderAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, Constants.Post, Route = "provider")] HttpRequest req,
            [Table(Constants.ProviderTableName)] CloudTable table,
            ILogger log)
        {
            try
            {
                if (!await _authService.IsAuthenticated(req))
                {
                    return new UnauthorizedResult();
                }

                var (model, errors) = await req.GetRequestModelAsync<ProviderViewModel, ProviderValidator>();
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
            catch (Exception ex)
            {
                log.LogCritical(ex, "Add-Provider caused an exception.");
                return ex.ToUnprocessableResult();
            }
        }

        [FunctionName("Get-Providers")]
        public async Task<IActionResult> GetProvidersAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, Constants.Get, Route = "providers")] HttpRequest req,
            [Table(Constants.ProviderTableName)] CloudTable table,
            ILogger log)
        {
            try
            {
                if (!await _authService.IsAuthenticated(req))
                {
                    return new UnauthorizedResult();
                }

                var query = new GetProvidersQuery();

                var providers = await table.ExecuteAsync(query);

                return new OkObjectResult(providers.Select(p => p.ToViewModel()));
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Get-Providers caused an exception.");
                return ex.ToServerError();
            }
        }

        [FunctionName("Delete-Provider")]
        public async Task<IActionResult> DeleteProviderAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, Constants.Delete, Route = "providers/{id}")] HttpRequest req,
            [Table(Constants.ProviderTableName)] CloudTable table,
            ILogger log,
            string id)
        {
            try
            {
                if (!await _authService.IsAuthenticated(req))
                {
                    return new UnauthorizedResult();
                }

                var command = new DeleteProviderCommand(id);

                await table.ExecuteAsync(command);

                return new NoContentResult();
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Delete-Provider caused an exception.");
                return ex.ToUnprocessableResult();
            }
        }
    }
}