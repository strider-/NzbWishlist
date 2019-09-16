using NzbWishlist.Core.Utilities;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace NzbWishlist.Core.Services
{
    public class DynamicHttpClientFactory : IHttpClientFactory
    {
        private readonly ConcurrentDictionary<string, HttpClient> _clients = new ConcurrentDictionary<string, HttpClient>();

        public HttpClient CreateClient(string name)
        {
            var client = _clients.GetOrAdd(name, new HttpClient(new ThrottlingMessageHandler(1, TimeSpan.FromSeconds(1))));

            return client;
        }
    }

    internal class ThrottlingMessageHandler : DelegatingHandler
    {
        private readonly TimeSpanSemaphore _timeSpanSemaphore;

        public ThrottlingMessageHandler(int maxRequests, TimeSpan perInterval)
            : this(new TimeSpanSemaphore(maxRequests, perInterval)) { }

        public ThrottlingMessageHandler(TimeSpanSemaphore timeSpanSemaphore) 
            : base(new HttpClientHandler())
        {
            _timeSpanSemaphore = timeSpanSemaphore;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _timeSpanSemaphore.RunAsync(base.SendAsync, request, cancellationToken);
        }
    }
}