﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using NzbWishlist.Azure.Extensions;
using NzbWishlist.Azure.Services;
using NzbWishlist.Core.Data;
using NzbWishlist.Core.Services;
using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NzbWishlist.Azure.Functions
{
    public class CartFunctions
    {
        private readonly INewznabClient _client;
        private readonly IAuthService _authService;

        public CartFunctions(INewznabClient client, IAuthService authService)
        {
            _client = client;
            _authService = authService;
        }

        [FunctionName("Cart-RSS")]
        public async Task<IActionResult> RssAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, Constants.Get, Route = "cart/rss")] HttpRequest req,
            [Table(Constants.CartTableName)] CloudTable table)
        {
            var location = new Uri(req.CreateLocation("/cart/rss", includeQuery: true));
            var feed = new SyndicationFeed("NzbWishlist", "NzbWishlist Feed", location);
            var sb = new StringBuilder();

            var entries = await table.ExecuteAsync(new GetCartQuery());

            feed.Links.Add(new SyndicationLink(location)
            {
                RelationshipType = "self",
                MediaType = "application/rss+xml"
            });
            feed.Items = entries.Select(e => e.ToSyndicationItem(req.QueryString));

            using (var writer = XmlWriter.Create(sb))
            {
                feed.SaveAsRss20(writer);
            }

            return new ContentResult
            {
                Content = sb.ToString(),
                ContentType = "text/xml",
                StatusCode = 200
            };
        }

        [FunctionName("Cart-Add")]
        public async Task<IActionResult> AddToCartAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, Constants.Post, Route = "cart/add/{wishResultId}")] HttpRequest req,
            [Table(Constants.WishTableName)] CloudTable wishTable,
            [Table(Constants.CartTableName)] CloudTable cartTable,
            ILogger log,
            string wishResultId)
        {
            try
            {
                if (!await _authService.IsAuthenticated(req))
                {
                    return new UnauthorizedResult();
                }

                var wishResult = await wishTable.ExecuteAsync(new GetWishResultQuery(wishResultId));

                var entry = wishResult.ToCartEntry(id => req.CreateLocation($"/cart/nzb/{id}"));

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
            [HttpTrigger(AuthorizationLevel.Anonymous, Constants.Get, Route = "cart/nzb/{cartId}")] HttpRequest req,
            [Table(Constants.CartTableName)] CloudTable table,
            ILogger log,
            string cartId)
        {
            try
            {
                var entry = await table.ExecuteAsync(new GetCartEntryQuery(cartId));
                if (entry == null)
                {
                    return new NotFoundResult();
                }

                var (nzbStream, nzbHeaders) = await _client.GetNzbStreamAsync(entry);
                if (nzbStream == null)
                {
                    return new NotFoundResult();
                }
                
                if (req.Query.TryGetValue("del", out var val) && val == "1")
                {
                    await table.ExecuteAsync(new RemoveFromCartCommand(entry));
                }

                foreach(var header in nzbHeaders)
                {
                    req.HttpContext.Response.Headers.Add(header);
                }
                return new FileStreamResult(nzbStream, "application/x+nzb");
            }
            catch (Exception ex)
            {
                log.LogCritical(ex, "Cart-Grab caused an exception.");
                return ex.ToServerError();
            }
        }
    }
}