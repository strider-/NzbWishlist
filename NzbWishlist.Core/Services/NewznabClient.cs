using NzbWishlist.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Services
{
    public class NewznabClient : INewznabClient
    {
        private IHttpClientFactory _factory;

        public NewznabClient(IHttpClientFactory factory) => _factory = factory;

        public async Task<IEnumerable<WishResult>> SearchAsync(Provider provider, Wish wish)
        {
            var client = GetHttpClient(provider.ApiUrl);
            var searchUrl = $"{provider.ApiUrl.TrimEnd('/')}/api?apikey={provider.ApiKey}&o=json&t=search&q={wish.Query}&maxage={wish.DaysSinceLastSearch()}";

            var resp = await client.GetAsync(searchUrl);

            if (!resp.IsSuccessStatusCode)
            {
                return Enumerable.Empty<WishResult>();
            }

            var searchResults = await resp.Content.ReadAsAsync<SearchResults>();
            var wishResults = new List<WishResult>();

            foreach (var item in searchResults.GetItems())
            {
                var result = item.ToWishResult();

                var possibleImageUrl = CreatePreviewUrl(provider, item.Guid);
                var imgPrevReq = new HttpRequestMessage(HttpMethod.Head, possibleImageUrl);
                var imgClient = GetHttpClient(possibleImageUrl);

                var imgResp = await imgClient.SendAsync(imgPrevReq);
                if (imgResp.IsSuccessStatusCode)
                {
                    result.PreviewUrl = possibleImageUrl;
                }

                wishResults.Add(result);
            }

            return wishResults;
        }

        public async Task<Stream> GetNzbStreamAsync(CartEntry entry)
        {
            var client = GetHttpClient(entry.NzbUrl);

            var resp = await client.GetAsync(entry.NzbUrl);
            if (!resp.IsSuccessStatusCode)
            {
                return null;
            }

            return await resp.Content.ReadAsStreamAsync();
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

        private HttpClient GetHttpClient(string url)
        {
            var uri = new Uri(url);
            return _factory.CreateClient(uri.Host);
        }
    }
}