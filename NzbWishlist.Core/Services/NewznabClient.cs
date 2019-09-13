﻿using NzbWishlist.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Services
{
    public class NewznabClient : INewznabClient
    {
        private HttpClient _client;

        public NewznabClient() => _client = new HttpClient();

        public async Task<IEnumerable<WishResult>> SearchAsync(Provider provider, Wish wish)
        {
            var searchUrl = $"{provider.ApiUrl.TrimEnd('/')}/api?apikey={provider.ApiKey}&o=json&t=search&q={wish.Query}&maxage={wish.DaysSinceLastSearch()}";

            var resp = await _client.GetAsync(searchUrl);

            if (!resp.IsSuccessStatusCode)
            {
                return Enumerable.Empty<WishResult>();
            }

            var searchResults = await resp.Content.ReadAsAsync<SearchResults>();
            var wishResults = new List<WishResult>();

            foreach (var item in searchResults.Items)
            {
                var result = item.ToWishResult();

                var possibleImageUrl = CreatePreviewUrl(provider, item.Guid);
                var imgPrevReq = new HttpRequestMessage(HttpMethod.Head, possibleImageUrl);

                var imgResp = await _client.SendAsync(imgPrevReq);
                if (imgResp.IsSuccessStatusCode)
                {
                    result.PreviewUrl = possibleImageUrl;
                }

                result.BelongsTo(wish);
                wishResults.Add(result);
            }

            return wishResults;
        }

        private string CreatePreviewUrl(Provider provider, string guid)
        {
            var builder = new UriBuilder(provider.ApiUrl);
            if (provider.ImageDomain != null)
            {
                builder.Host = provider.ImageDomain;
            }

            builder.Path = $"/covers/preview/{guid}_thumb.jpg";

            return builder.ToString();
        }
    }
}